﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Diagnostics;
using LAMA.Singletons;
using Xamarin.Forms;
using Mapsui.Providers.Wms;
using LAMA.Models;
using LAMA.ViewModels;

namespace LAMA.Communicator
{
    /// <summary>
    /// Handles the communication for client applications. 
    /// It is capable of sending messages to the server and reacting to those arriving from the server.
    /// </summary>
    public class ClientCommunicator : Communicator
    {
        public Compression CompressionManager { get; set; }
        private bool _connected = false;
        /// <summary>
        /// Did we manage to establish a connection with the server?
        /// </summary>
        public bool connected
        {
            get { return _connected; }
        }
        /// <summary>
        /// Is the client already logged in into the application
        /// </summary>
        public bool loggedIn = false;
        /// <summary>
        /// Server socket
        /// </summary>
        public static Socket s;
        //So we can use the socket in different threads
        private static object socketLock = new object();
        //Thread the client listens on
        private Thread listener;
        /// <summary>
        /// When was the client last updated from the server
        /// </summary>
        public long LastUpdate
        {
            get { return LocalStorage.LastUpdateTime; }
            set { if (wasUpdated) { LocalStorage.LastUpdateTime = value; } }
        }
        private bool wasUpdated = false;

        //TCP connection address
        private IPAddress _IP;
        private int _port;
        private bool _IPv6 = false;
        private bool _IPv4 = false;

        //Buffer for received messages
        static byte[] buffer = new byte[8 * 1024];
        private static ClientCommunicator THIS;
        //Memory
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;
        private RememberedStringDictionary<ModelPropertyChangeInfo, ModelPropertyChangeInfoStorage> attributesCache;
        private ModelChangesManager modelChangesManager;
        static Random rd = new Random();

        //Queue of broadcasted commands and its lock
        private object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();

        //Timers for periodic connection attempts and message sending
        private Timer connectionTimer;
        private Timer broadcastTimer;

        /// <summary>
        /// When the server refuses a client it send a message specifying the reason why.
        /// </summary>
        public string clientRefusedMessage = "";

        /// <summary>
        /// Stop communication gracefully. Clean up, dispose of the System.Threading.Timers broadcasting messages.
        /// </summary>
        public void EndCommunication()
        {
            if (s != null)
            {
                s.Dispose();
            }
            if (connectionTimer != null) connectionTimer.Dispose();
            if (broadcastTimer != null) broadcastTimer.Dispose();
        }

        /// <summary>
        /// Stop trying to connect to the server.
        /// </summary>
        public void KillConnectionTimer()
        {
            if (connectionTimer != null)
            {
                connectionTimer.Dispose();
                connectionTimer = null;
            }
        }
        /// <summary>
        /// Sends all the messages in the buffer (commands to broadcast) to the server
        /// </summary>
        private void ProcessBroadcast()
        {
            lock (socketLock)
            {
                if (s.Connected)
                {
                    lock (commandsLock)
                    {
                        while (commandsToBroadcast.Count > 0)
                        {
                            Command currentCommand = commandsToBroadcast.Peek();
                            if (!connected && !currentCommand.command.StartsWith("RequestID") && !currentCommand.command.StartsWith("Connected"))
                            {
                                commandsToBroadcast.Dequeue();
                                commandsToBroadcast.Enqueue(currentCommand);
                                break;
                            }
                            Debug.WriteLine($"Sending: {currentCommand.command}");
                            byte[] data = currentCommand.Encode(CompressionManager);
                            try
                            {
                                s.Send(data);
                                commandsToBroadcast.Dequeue();
                            }
                            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                            {
                                s.Close();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    s.Close();
                }
            }
            Device.BeginInvokeOnMainThread(new Action(() =>
            {
                SaveCommandQueue();
            }));
        }

        /// <summary>
        /// Queues a command (into commandsToBroadcast) which will then be sent to the server
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(Command command)
        {
            int savedQueueLength = Int32.Parse(objectsCache.getByKey("CommandQueueLength").command);
            string key = "CommandQueue" + savedQueueLength;
            objectsCache.getByKey("CommandQueueLength").command = (savedQueueLength + 1).ToString();
            objectsCache.add(new Command(command.command, command.time, key, command.receiverID));
            lock (commandsLock)
            {
                commandsToBroadcast.Enqueue(command);
                SaveCommandQueue();
            }
        }

        /// <summary>
        /// Decodes the messages received over the network and sends it to the methods responding to the data
        /// </summary>
        /// <param name="AR"></param>
        /// <exception cref="SocketException"></exception>
        private static void ReceiveData(IAsyncResult AR)
        {
            int received;
            Socket current = (Socket)AR.AsyncState;
            //Try to receive data, if there is a problem close connection (will try to reconnect immediatelly)
            lock (ClientCommunicator.socketLock)
            {
                try
                {
                    received = current.EndReceive(AR);
                    if (received == 0)
                    {
                        throw new SocketException();
                    }
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    current.Close();
                    return;
                }
            }
            byte[] data = new byte[received];
            Array.Copy(buffer, data, received);
            string[] messages = Encryption.AESDecryptHuffmanDecompress(data, THIS.CompressionManager).Split(SpecialCharacters.messageSeparator);

            for (int i = 0; i < messages.Length - 1; i++)
            {
                Debug.WriteLine($"Received: {messages[i]}");
                string message = messages[i];
                message = message.Trim('\0');
                string[] messageParts = message.Split(SpecialCharacters.messagePartSeparator);
                for (int j = 0; j < messageParts.Length; j++)
                {
                    if (messageParts[j].Length > 0 && messageParts[j][messageParts[j].Length - 1] == 'Â')
                    {
                        messageParts[j] = messageParts[j].Remove(messageParts[j].Length - 1);
                    }
                }
                if (messageParts[1] == "Rollback" || messageParts[1] == "DataUpdated" || messageParts[1] == "ItemCreated" || messageParts[1] == "ItemDeleted" || messageParts[1] == "CPLocations" 
                    || messageParts[1] == "ReceiveRole" || messageParts[1] == "RemoveRoleResult" || messageParts[1] == "ChannelCreatedResult" || messageParts[1] == "ChannelModifiedResult")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.modelChangesManager.ProcessCommand(message, current);
                    }));
                }
                //Received IDs from the server
                if (messageParts[1] == "GiveID")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.ReceiveID(Int32.Parse(messageParts[2]), Int32.Parse(messageParts[3]));
                    }));
                }
                //Managed to connect to the server
                if (messageParts[1] == "Connected")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.Connected();
                    }));
                }
                //Client was refused by the server
                if (messageParts[1] == "ClientRefused")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.ClientRefused(Int32.Parse(messageParts[2]), messageParts[3]);
                    }));
                }
                if (messageParts[1] == "UpdateFinished")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.UpdateFinished();
                    }));
                }
            }
            try
            {
                current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (ClientCommunicator.socketLock)
                {
                    current.Close();
                }
                return;
            }
        }

        /// <summary>
        /// Client received a client and CP id from the server
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="cpId"></param>
        private void ReceiveID(int clientId, int cpId)
        {
            if (LocalStorage.clientID == -1)
            {
                LocalStorage.clientID = clientId;
            }
            if (LocalStorage.cpID == -1) {
                LocalStorage.cpID = cpId;
                RequestUpdate();
            }
        }

        private void UpdateFinished()
        {
            loggedIn = true;
        }
        
        /// <summary>
        /// Client was refused by the server (incorrect password, or the client already exists)
        /// </summary>
        /// <param name="clientId"></param>
        private void ClientRefused(int clientId, string message)
        {
            if (LocalStorage.clientID == -1)
            {
                loggedIn = false;
                LocalStorage.clientID = clientId;
            }
            clientRefusedMessage = message;
        }

        /// <summary>
        /// Request update from the server
        /// </summary>
        private void RequestUpdate()
        {
            string q = "";
            q += $"Update{SpecialCharacters.messagePartSeparator}";
            q += LocalStorage.clientID;
            SendCommand(new Command(q, LastUpdate, "None"));
        }

        /// <summary>
        /// Start receiving data from the server
        /// </summary>
        private void StartListening()
        {
            lock (ClientCommunicator.socketLock)
            {
                try
                {
                    s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), s);
                }
                catch (Exception ex) when(ex is SocketException || ex is ObjectDisposedException)
                {
                    s.Close();
                    return;
                }
            }
        }

        /// <summary>
        /// Start periodically broadcasting messages in the buffer
        /// </summary>
        private void StartBroadcasting()
        {
            broadcastTimer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

        /// <summary>
        /// Create socket for server communication
        /// </summary>
        private void InitSocket()
        {
            lock (ClientCommunicator.socketLock)
            {
                if (_IPv6)
                {
                    s = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                }
                else
                {
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            }
        }

        /// <summary>
        /// Log into the server as a CP
        /// </summary>
        /// <param name="cpName"></param>
        /// <param name="password"></param>
        /// <param name="isNew">is this a new CP or a CP, that already exists on the server</param>
        public void LoginAsCP(string cpName, string password, bool isNew = true)
        {
            if (LocalStorage.cpID == -1)
            {
                LocalStorage.clientName = cpName;
            }
            clientRefusedMessage = "";
            if (isNew)
            {
                SendCommand(new Command(
                    $"RequestID{SpecialCharacters.messagePartSeparator}{cpName}{SpecialCharacters.messagePartSeparator}{Encryption.EncryptPassword(password)}{SpecialCharacters.messagePartSeparator}{LocalStorage.clientID}", 
                    DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
            }
            else
            {
                SendCommand(new Command(
                    $"RequestIDExisting{SpecialCharacters.messagePartSeparator}{cpName}{SpecialCharacters.messagePartSeparator}{Encryption.EncryptPassword(password)}{SpecialCharacters.messagePartSeparator}{LocalStorage.clientID}", 
                    DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
            }
            if (broadcastTimer == null)
            {
                StartBroadcasting();
            }
            //If not already listening start listening to the data from the server
            if (listener == null)
            {
                listener = new Thread(StartListening);
                listener.Start();
            }
        }

        /// <summary>
        /// Try to connect to the server (if not already connected)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServerConnectionRefusedException"></exception>
        private bool Connect()
        {
            lock (ClientCommunicator.socketLock)
            {
                if (!s.Connected)
                {
                    wasUpdated = false;
                    if (listener != null)
                    {
                        listener = null;
                    }
                    try
                    {
                        InitSocket();
                        s.Connect(_IP, _port);
                        if (loggedIn && LocalStorage.clientID != -1)
                        {
                            SendCommand(new Command($"ClientConnected{SpecialCharacters.messagePartSeparator}{LocalStorage.clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
                            RequestUpdate();
                            listener = new Thread(StartListening);
                            listener.Start();
                        }
                    }
                    catch (SocketException e)
                    {
                        if (e.Message == "Connection refused")
                        {
                            throw new ServerConnectionRefusedException("Server refused the connection, check your firewall settings");
                        }
                    }
                    catch
                    {
                        Device.BeginInvokeOnMainThread(new Action(() =>
                        {
                            THIS._connected = false;
                            THIS.wasUpdated = false;
                        }));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Server said, that we successfully connected
        /// </summary>
        private void Connected()
        {
            if (LocalStorage.clientID != -1 )
                _connected = true;
                THIS.wasUpdated = true;
        }

        /// <summary>
        /// Save broadcasted messages to a database
        /// </summary>
        private void SaveCommandQueue()
        {
            List<Command> commandsToSave = new List<Command>();
            lock (commandsLock) {
                commandsToSave = commandsToBroadcast.ToList();
            }
            objectsCache.getByKey("CommandQueueLength").command = (commandsToBroadcast.Count).ToString();
            for (int i = 0; i < commandsToSave.Count; i++)
            {
                string key = "CommandQueue" + i;
                if (objectsCache.getByKey(key) != null)
                {
                    objectsCache.getByKey(key).command = commandsToSave[i].command;
                }
                else
                {
                    objectsCache.add(new Command(commandsToSave[i].command, commandsToSave[i].time, key, commandsToSave[i].receiverID));
                }
            }
        }

        /// <summary>
        /// Load messages saved in the database
        /// </summary>
        private void LoadCommandQueue()
        {
            List<Command> commandsLoaded = new List<Command>();
            int savedQueueLength = Int32.Parse(objectsCache.getByKey("CommandQueueLength").command);
            for (int i = 0; i < savedQueueLength; i++)
            {
                string key = "CommandQueue" + i;
                if (objectsCache.getByKey(key) != null)
                {
                    commandsLoaded.Add(objectsCache.getByKey(key));
                }
            }

            lock (commandsLock)
            {
                commandsToBroadcast = new Queue<Command>(commandsLoaded);
            }
        }

        /// <summary>
        /// Create a client communicator. A class capable of sending messages to the server based on the changes happening on the
        /// client side, receiving messages from the server and updating the status of the program based on the data received.
        /// </summary>
        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongCredentialsException">Wrong password used</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="ServerConnectionRefusedException">The server refused your connection</exception>
        public ClientCommunicator(string serverName, string password)
        {
            CompressionManager = new Compression();
            _connected = false;


            //Try to connect to the central server to get information about the LARP server
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + serverName + "\"" },
                { "password", "\"" + Encryption.EncryptPassword(password) + "\"" }
            };

            var content = new FormUrlEncodedContent(values);
            var responseString = "";
            try
            {
                var response = client.PostAsync("https://koblizekwebdesign.cz/LAMA/findserver.php", content);
                responseString = response.Result.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException)
            {
                throw new CantConnectToCentralServerException("Nepodařilo se připojit k centrálnímu serveru, zkontrolujte si prosím vaše internetové připojení.");
            }
            if (responseString == "Connection")
            {
                throw new CantConnectToDatabaseException("Nepodařilo se připojit k databázi.");
            }
            else if (responseString == "credintials")
            {
                throw new WrongCredentialsException("Špatné heslo nebo neexistující server.");
            }
            //Managed to connect to the central server database and the LARP server exists
            else
            {
                CommunicationInfo.Instance.Communicator = this;
                CommunicationInfo.Instance.ServerName = serverName;
                CommunicationInfo.Instance.IsServer = false;

                //if (serverName != LarpEvent.Name && LarpEvent.Name != null) SQLConnectionWrapper.ResetDatabase();
                LarpEvent.Name = serverName;
                attributesCache = DatabaseHolderStringDictionary<ModelPropertyChangeInfo, ModelPropertyChangeInfoStorage>.Instance.rememberedDictionary;
                objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
                if (objectsCache.getByKey("CommandQueueLength") == null)
                {
                    objectsCache.add(new Command("0", "CommandQueueLength"));
                }
                LoadCommandQueue();
                Encryption.SetAESKey(password + serverName + "abcdefghijklmnopqrstu123456789qwertzuiop");
                string[] array = responseString.Split(',');
                //Get server IP (check if it is valid)
                if (IPAddress.TryParse(array[0].Trim('"'), out _IP))
                {
                    if (_IP.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        _IPv6 = true;
                    }
                    else
                    {
                        _IPv4 = true;
                    }
                }
                else
                {
                    throw new NotAnIPAddressException($"IP adresa serveru není validní {array[0].Trim('"')}");
                }
                THIS = this;
                _port = int.Parse(array[1]);
                LocalStorage.serverName = serverName;
                LocalStorage.clientID = -1;
                LocalStorage.cpID = -1;
                InitSocket();

                //Periodically try to connect to the LARP server
                connectionTimer = new System.Threading.Timer((e) =>
                {
                    Connect();
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2000));


                THIS.LastUpdate = LocalStorage.LastUpdateTime;
                THIS.wasUpdated = false;
                modelChangesManager = new ModelChangesManager(this, objectsCache, attributesCache);
                SQLEvents.dataChanged += modelChangesManager.OnDataUpdated;
                SQLEvents.created += modelChangesManager.OnItemCreated;
                SQLEvents.dataDeleted += modelChangesManager.OnItemDeleted;
                ActivityDetailsViewModel.roleRequested += modelChangesManager.OnRoleRequested;
                ActivityDetailsViewModel.roleRemoved += modelChangesManager.OnRoleRemoved;
                ChatChannelsViewModel.channelCreated += modelChangesManager.OnChannelCreated;
                ChatChannelsViewModel.channelModified += modelChangesManager.OnChannelModified;
            }
        }
    }
}
