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
        public DebugLogger Logger
        {
            get { return logger; }
        }
        private bool _connected = false;
        public bool connected
        {
            get { return _connected; }
        }
        public bool loggedIn = false;
        public static Socket s;
        private static object socketLock = new object();
        private Thread listener;
        public long lastUpdate;
        public long LastUpdate
        {
            get { return lastUpdate; }
            set { if (wasUpdated) { lastUpdate = value; } }
        }
        private bool wasUpdated = false;
        //private static string CPName;
        //private static string assignedEvent = "";
        private IPAddress _IP;
        private int _port;
        private bool _IPv6 = false;
        private bool _IPv4 = false;

        static byte[] buffer = new byte[8 * 1024];
        private static ClientCommunicator THIS;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;
        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private ModelChangesManager modelChangesManager;
        static Random rd = new Random();

        private object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();

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

        private static void ReceiveData(IAsyncResult AR)
        {
            int received;
            Socket current = (Socket)AR.AsyncState;
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
                if (messageParts[1] == "GiveID")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.ReceiveID(Int32.Parse(messageParts[2]), Int32.Parse(messageParts[3]));
                    }));
                }
                if (messageParts[1] == "Connected")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.Connected();
                    }));
                }
                if (messageParts[1] == "ClientRefused")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.ClientRefused(Int32.Parse(messageParts[2]));
                    }));
                }
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

        private void ReceiveID(int clientId, int cpId)
        {
            loggedIn = true;
            LocalStorage.clientID = clientId;
            LocalStorage.cpID = cpId;
            Debug.WriteLine($"clientID: {clientId}, cpID: {cpId}");
            RequestUpdate();
        }

        private void ClientRefused(int clientId)
        {
            loggedIn = true;
            LocalStorage.clientID = clientId;
        }

        private void RequestUpdate()
        {
            string q = "";
            q += "Update;";
            q += LocalStorage.clientID;
            SendCommand(new Command(q, lastUpdate, "None"));
        }

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

        private void StartBroadcasting()
        {
            broadcastTimer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

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
            if (listener == null)
            {
                listener = new Thread(StartListening);
                listener.Start();
            }
        }

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

        private void Connected()
        {
            logger.LogWrite("connected set true");
            _connected = true;
            THIS.wasUpdated = true;
        }

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

        private void SendClientInfo()
        {
            string command = "ClientConnected" + ";" + LocalStorage.clientID;
            SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None"));
        }

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
            logger.LogWrite("Created dictionaries");

            if (objectsCache.getByKey("CommandQueueLength") == null)
            {
                objectsCache.add(new Command("0", "CommandQueueLength"));
            }
            LoadCommandQueue();
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
            else
            {
                logger.LogWrite("No exceptions");
                Encryption.SetAESKey(password + serverName + "abcdefghijklmnopqrstu");
                string[] array = responseString.Split(',');
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
                //Connect();
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
            logger.LogWrite("Created dictionaries");

            if (objectsCache.getByKey("CommandQueueLength") == null)
            {
                objectsCache.add(new Command("0", "CommandQueueLength"));
            }
            LoadCommandQueue();
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
            else
            {
                logger.LogWrite("No exceptions");
                string[] array = responseString.Split(',');
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
