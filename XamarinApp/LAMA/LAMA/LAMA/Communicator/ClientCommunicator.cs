using System;
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

namespace LAMA.Communicator
{
    public class ClientCommunicator : Communicator
    {
        public DebugLogger logger;
        //Logging into a text file
        public DebugLogger Logger
        {
            get { return logger; }
        }
        //Managed to connect to the server
        private bool _connected = false;
        public bool connected
        {
            get { return _connected; }
        }
        //Is the client already logged in into the application
        public bool loggedIn = false;
        //Server socket
        public static Socket s;
        //So we can use the socket in different threads
        private static object socketLock = new object();
        //Thread the client listens on
        private Thread listener;
        //When was the client last updated from the server
        public long lastUpdate;
        public long LastUpdate
        {
            get { return lastUpdate; }
            set { if (wasUpdated) { lastUpdate = value; } }
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
        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private ModelChangesManager modelChangesManager;
        static Random rd = new Random();

        //Queue of broadcasted commands and its lock
        private object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();

        //Timers for periodicconnection attempts and message sending
        private Timer connectionTimer;
        private Timer broadcastTimer;

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
                            logger.LogWrite(commandsToBroadcast.Count.ToString());
                            logger.LogWrite(connected.ToString());
                            Command currentCommand = commandsToBroadcast.Peek();
                            if (!connected && !currentCommand.command.StartsWith("GiveID") && !currentCommand.command.StartsWith("Connected"))
                            {
                                commandsToBroadcast.Dequeue();
                                commandsToBroadcast.Enqueue(currentCommand);
                                break;
                            }
                            logger.LogWrite($"Sending: {currentCommand.command}");
                            Debug.WriteLine($"Sending: {currentCommand.command}");
                            byte[] data = currentCommand.Encode();
                            Debug.WriteLine($"{Encryption.DecryptStringFromBytes_Aes(data)}");
                            try
                            {
                                s.Send(data);
                                commandsToBroadcast.Dequeue();
                                logger.LogWrite($"Finished Sending: {currentCommand.command}");
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
            logger.LogWrite($"Sending Command: {command.command} | {command.time} | {command.key} | {command.receiverID}");
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
            Debug.WriteLine($"Encoded message: {System.Convert.ToBase64String(data)}");
            Debug.WriteLine($"Message string received: {Encryption.DecryptStringFromBytes_Aes(data)}");
            string[] messages = Encryption.DecryptStringFromBytes_Aes(data).Split('|');

            for (int i = 0; i < messages.Length - 1; i++)
            {
                string message = messages[i];
                message = message.Trim('\0');
                THIS.logger.LogWrite($"Message Received: {message}");
                Debug.WriteLine($"Message Received: {message}");
                string[] messageParts = message.Split(';');
                for (int j = 0; j < messageParts.Length; j++)
                {
                    if (messageParts[j].Length > 0 && messageParts[j][messageParts[j].Length - 1] == 'Â')
                    {
                        messageParts[j] = messageParts[j].Remove(messageParts[j].Length - 1);
                    }
                }
                if (messageParts[1] == "DataUpdated")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.LastUpdate = Int64.Parse(messageParts[0]);
                        THIS.modelChangesManager.DataUpdated(messageParts[2], Int64.Parse(messageParts[3]), Int32.Parse(messageParts[4]), messageParts[5], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1), current);
                    }));
                }
                if (messageParts[1] == "ItemCreated")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.modelChangesManager.ItemCreated(messageParts[2], messageParts[3], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1), current);
                    }));
                }
                if (messageParts[1] == "ItemDeleted")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.LastUpdate = Int64.Parse(messageParts[0]);
                        THIS.modelChangesManager.ItemDeleted(messageParts[2], Int64.Parse(messageParts[3]), Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
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
                        THIS.ClientRefused(Int32.Parse(messageParts[2]));
                    }));
                }
                //Rollback effects of previous messages (data updates and item creations)
                if (messageParts[1] == "Rollback")
                {
                    if (messageParts[2] == "DataUpdated")
                    {
                        THIS.LastUpdate = Int64.Parse(messageParts[0]);
                        THIS.modelChangesManager.RollbackDataUpdated(messageParts[3], Int64.Parse(messageParts[4]), Int32.Parse(messageParts[5]), messageParts[6], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                    }
                    if (messageParts[2] == "ItemCreated")
                    {
                        THIS.LastUpdate = Int64.Parse(messageParts[0]);
                        THIS.modelChangesManager.RollbackItemCreated(messageParts[3], messageParts[4], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                    }
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
            loggedIn = true;
            LocalStorage.clientID = clientId;
            LocalStorage.cpID = cpId;
            Debug.WriteLine($"clientID: {clientId}, cpID: {cpId}");
            RequestUpdate();
        }
        
        /// <summary>
        /// Client was refused by the server (incorrect password, or the client already exists)
        /// </summary>
        /// <param name="clientId"></param>
        private void ClientRefused(int clientId)
        {
            loggedIn = true;
            LocalStorage.clientID = clientId;
        }

        /// <summary>
        /// Request update from the server
        /// </summary>
        private void RequestUpdate()
        {
            string q = "";
            q += "Update;";
            q += LocalStorage.clientID;
            SendCommand(new Command(q, lastUpdate, "None"));
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

        /*
        private bool Connect()
        {
            lock (ClientCommunicator.socketLock)
            {
                if (!s.Connected)
                {
                    if (listener != null)
                    {
                        listener = null;
                    }
                    try
                    {
                        logger.LogWrite("Trying to connect");
                        InitSocket();
                        s.Connect(_IP, _port);
                        logger.LogWrite("Connected");
                        if (LocalStorage.clientID == -1)
                        {
                            SendCommand(new Command($"GiveID;{LocalStorage.clientName}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
                        }
                        else
                        {
                            SendCommand(new Command($"ClientConnected;{LocalStorage.clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
                        }
                        if (broadcastTimer == null)
                        {
                            StartBroadcasting();
                        }
                        if (listener == null)
                        {
                            listener = new Thread(StartListening);
                            listener.Start();
                        }
                    }
                    catch (SocketException e)
                    {
                        logger.LogWrite(e.Message);
                        if (e.Message == "Connection refused")
                        {
                            throw new ServerConnectionRefusedException("Server refused the connection, check your port forwarding and firewall settings");
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
        */

        /// <summary>
        /// Log into the server as a CP
        /// </summary>
        /// <param name="cpName"></param>
        /// <param name="password"></param>
        /// <param name="isNew">is this a new CP or a CP, that already exists on the server</param>
        public void LoginAsCP(string cpName, string password, bool isNew = true)
        {
            LocalStorage.clientName = cpName;
            if (isNew)
            {
                SendCommand(new Command($"GiveID;{cpName};{Encryption.EncryptPassword(password)};{LocalStorage.clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
            }
            else
            {
                SendCommand(new Command($"GiveIDExisting;{cpName};{Encryption.EncryptPassword(password)};{LocalStorage.clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
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
                            SendCommand(new Command($"ClientConnected;{LocalStorage.clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
                        }
                    }
                    catch (SocketException e)
                    {
                        logger.LogWrite(e.Message);
                        if (e.Message == "Connection refused")
                        {
                            throw new ServerConnectionRefusedException("Server refused the connection, check your port forwarding and firewall settings");
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
            logger.LogWrite("connected set true");
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
        /// Send information, that client connected to the server (specify the client by its ID)
        /// </summary>
        private void SendClientInfo()
        {
            string command = "ClientConnected" + ";" + LocalStorage.clientID;
            SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
        }
        /// <summary>
        /// Create a client communicator. A class capable of sending messages to the server based on the changes happening on the
        /// client side, receiving messages from the server and updating the status of the program based on the data received.
        /// </summary>
        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongCreadintialsException">Wrong password used</exception>
        /// <exception cref="NonExistentServerException">Server with this name doesn't exist</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="ServerConnectionRefusedException">The server refused your connection</exception>
        public ClientCommunicator(string serverName, string password)
        {
            if (serverName != LarpEvent.Name && LarpEvent.Name != null) SQLConnectionWrapper.ResetDatabase();
            Debug.WriteLine("client communicator");
            LarpEvent.Name = serverName;
            logger = new DebugLogger(false);
            _connected = false;

            attributesCache = DatabaseHolderStringDictionary<TimeValue, TimeValueStorage>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
            if (objectsCache.getByKey("CommandQueueLength") == null)
            {
                objectsCache.add(new Command("0", "CommandQueueLength"));
            }
            LoadCommandQueue();

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
                throw new WrongCreadintialsException("Špatné heslo nebo neexistující server.");
            }
            //Managed to connect to the central server database and the LARP server exists
            else
            {
                logger.LogWrite("No exceptions");
                Encryption.SetAESKey(password + serverName + "abcdefghijklmnopqrstu");
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
                    throw new NotAnIPAddressException("IP adresa serveru není validní");
                }
                THIS = this;
                _port = int.Parse(array[1]);
                LocalStorage.serverName = serverName;
                LocalStorage.clientID = -1;
                InitSocket();

                //Periodically try to connect to the LARP server
                connectionTimer = new System.Threading.Timer((e) =>
                {
                    Connect();
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2000));


                THIS.lastUpdate = 0;
                THIS.wasUpdated = false;
                modelChangesManager = new ModelChangesManager(this, objectsCache, attributesCache);
                logger.LogWrite("Subscribing to events");
                SQLEvents.dataChanged += modelChangesManager.OnDataUpdated;
                SQLEvents.created += modelChangesManager.OnItemCreated;
                SQLEvents.dataDeleted += modelChangesManager.OnItemDeleted;
                logger.LogWrite("Initialization finished");
            }
        }

        /// <summary>
        /// Create a client communicator. A class capable of sending messages to the server based on the changes happening on the
        /// client side, receiving messages from the server and updating the status of the program based on the data received.
        /// </summary>
        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongCreadintialsException">Wrong password used</exception>
        /// <exception cref="NonExistentServerException">Server with this name doesn't exist</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="ServerConnectionRefusedException">The server refused your connection</exception>
        public ClientCommunicator(string serverName, string password, string clientName)
        {
            if (serverName != LarpEvent.Name && LarpEvent.Name != null) SQLConnectionWrapper.ResetDatabase();
            Debug.WriteLine("client communicator");
            LarpEvent.Name = serverName;
            logger = new DebugLogger(false);
            _connected = false;

            attributesCache = DatabaseHolderStringDictionary<TimeValue, TimeValueStorage>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
            if (objectsCache.getByKey("CommandQueueLength") == null)
            {
                objectsCache.add(new Command("0", "CommandQueueLength"));
            }
            LoadCommandQueue();

            //Try to connect to the central server to find information about the LARP server
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
                throw new WrongCreadintialsException("Špatné heslo, nebo neexistující server.");
            }
            //Managed to connect to the central server database and LARP server exists
            else
            {
                logger.LogWrite("No exceptions");
                string[] array = responseString.Split(',');
                //Get server IP address (check if it is valid)
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
                    throw new NotAnIPAddressException("IP adresa serveru není validní");
                }
                THIS = this;
                _port = int.Parse(array[1]);
                LocalStorage.clientName = clientName;
                LocalStorage.serverName = serverName;
                LocalStorage.clientID = -1;
                InitSocket();
                //Periodically try to connect to the server
                Connect();
                connectionTimer = new System.Threading.Timer((e) =>
                {
                    Connect();
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2000));


                THIS.lastUpdate = 0;
                THIS.wasUpdated = false;
                modelChangesManager = new ModelChangesManager(this, objectsCache, attributesCache);
                logger.LogWrite("Subscribing to events");
                SQLEvents.dataChanged += modelChangesManager.OnDataUpdated;
                SQLEvents.created += modelChangesManager.OnItemCreated;
                SQLEvents.dataDeleted += modelChangesManager.OnItemDeleted;
                logger.LogWrite("Initialization finished");
            }
        }
    }
}
