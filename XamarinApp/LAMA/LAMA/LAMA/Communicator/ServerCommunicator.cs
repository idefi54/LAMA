using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Essentials;
using System.Diagnostics;
using LAMA.Models;
using LAMA.Singletons;
using Xamarin.Forms;
using SQLite;
using System.Threading.Tasks;

namespace LAMA.Communicator
{
    public class ServerCommunicator : Communicator
    {
        public DebugLogger logger;
        public DebugLogger Logger
        {
            get { return logger; }
        }
        public Compression CompressionManager { get; set; }

        public long LastUpdate
        {
            get { return DateTimeOffset.Now.ToUnixTimeMilliseconds(); }
            set { }
        }

        static byte[] buffer = new byte[8 * 1024];
        static Socket serverSocket;
        private Thread server;
        private static ServerCommunicator THIS;
        private Timer timer;
        CancellationTokenSource tokenLocationSending;

        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;

        private static object socketsLock = new object();
        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();

        private static object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();
        private Command currentCommand = null;

        private ModelChangesManager modelChangesManager;

        public void EndCommunication()
        {
            foreach (Socket clientSocket in clientSockets.Values)
            {
                if (clientSocket != null)
                {
                    //clientSocket.Disconnect(true);
                    clientSocket.Dispose();
                }
            }
            if (serverSocket != null)
            {
                //serverSocket.Disconnect(true);
                serverSocket.Dispose();
            }
            if (timer != null) timer.Dispose();
            if (server != null) server.Abort();
            if (tokenLocationSending != null)
            {
                try
                {
                    tokenLocationSending.Cancel();
                    tokenLocationSending.Dispose();
                }
                catch (ObjectDisposedException) { }
            }
            clientSockets = new Dictionary<int, Socket>();
        }

        /// <summary>
        /// Start accepting clients and broadcasting messages to them
        /// </summary>
        private async void StartServer()
        {
            Debug.WriteLine("Starting Server");
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Debug.WriteLine("BeginAccept");

            timer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

            tokenLocationSending = new CancellationTokenSource();
            CancellationToken token = tokenLocationSending.Token;
            try
            {
                await SendLocations(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        public async Task SendLocations(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(30_000);
                    token.ThrowIfCancellationRequested();
                    Device.BeginInvokeOnMainThread(() => modelChangesManager.SendCPLocations());
                }
            }, token);
        }

        /// <summary>
        /// Broadcast messages to the connected clients
        /// </summary>
        private void ProcessBroadcast()
        {
            while (true)
            {
                lock (ServerCommunicator.commandsLock)
                {
                    if (commandsToBroadcast.Count > 0)
                    {
                        currentCommand = commandsToBroadcast.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
                if (currentCommand != null)
                {
                    logger.LogWrite($"Sending: {currentCommand.command}");
                    Debug.WriteLine($"Sending: {currentCommand.command}");
                    byte[] data = currentCommand.Encode(CompressionManager);
                    Debug.WriteLine($"{Encryption.AESDecryptHuffmanDecompress(data, CompressionManager)}");
                    List<int> socketsToRemove = new List<int>();
                    lock (ServerCommunicator.socketsLock)
                    {
                        //Send message to all the clients connected
                        if (currentCommand.receiverID == 0)
                        {
                            foreach (KeyValuePair<int, Socket> entry in clientSockets)
                            {
                                Socket client = entry.Value;
                                if (client.Connected)
                                {
                                    try
                                    {
                                        client.Send(data);
                                    }
                                    catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                                    {
                                        Debug.WriteLine("Socket exception ProcessBroadcast");
                                        client.Close();
                                        socketsToRemove.Add(entry.Key);
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogWrite(ex.Message);
                                    }
                                }
                                else
                                {
                                    client.Close();
                                    socketsToRemove.Add(entry.Key);
                                }
                            }
                        }
                        //Send message only to one client
                        else if (clientSockets.ContainsKey(currentCommand.receiverID))
                        {
                            Socket client = clientSockets[currentCommand.receiverID];
                            if (client.Connected)
                            {
                                try
                                {
                                    client.Send(data);
                                }
                                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                                {
                                    Debug.WriteLine("Socket exception ProcessBroadcast2");
                                    client.Close();
                                    socketsToRemove.Add(currentCommand.receiverID);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWrite(ex.Message);
                                }
                            }
                            else
                            {
                                client.Close();
                                socketsToRemove.Add(currentCommand.receiverID);
                            }
                        }
                        foreach (int i in socketsToRemove)
                        {
                            logger.LogWrite($"Removing From Client Sockets: {i}");
                            clientSockets.Remove(i);
                        }
                    }
                    logger.LogWrite($"Finished Sending: {currentCommand.command}");
                }
            }
        }

        /// <summary>
        /// React to the clients connecting to the server
        /// </summary>
        /// <param name="AR"></param>
        private static void AcceptCallback(IAsyncResult AR)
        {
            THIS.logger.LogWrite("accepting");
            Debug.WriteLine("accepting");
            try
            {
                Socket socket = serverSocket.EndAccept(AR);
                try
                {
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), socket);
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.logger.LogWrite("Client socket exception");
                        Debug.WriteLine("Client socket exception AcceptCallback");
                    }));
                    lock (ServerCommunicator.socketsLock)
                    {
                        socket.Close();
                        foreach (var item in clientSockets.Where(x => x.Value == socket).ToList())
                        {
                            clientSockets.Remove(item.Key);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// React to the messages from the clients
        /// </summary>
        /// <param name="AR"></param>
        /// <exception cref="SocketException"></exception>
        private static void ReceiveData(IAsyncResult AR)
        {
            int received;
            Socket current = (Socket)AR.AsyncState;
            try
            {
                received = current.EndReceive(AR);
                if (received == 0)
                {
                    Debug.WriteLine("SocketException ReceiveData");
                    throw new SocketException();
                }
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                Debug.WriteLine("SocketException ReceiveData");
                lock (ServerCommunicator.socketsLock)
                {
                    current.Close();
                    foreach (var item in clientSockets.Where(x => x.Value == current).ToList())
                    {
                        clientSockets.Remove(item.Key);
                    }
                }
                return;
            }
            byte[] data = new byte[received];
            Array.Copy(buffer, data, received);
            Debug.WriteLine($"Message String Received: {Encryption.AESDecryptHuffmanDecompress(data, THIS.CompressionManager)}");
            string[] messages = Encryption.AESDecryptHuffmanDecompress(data, THIS.CompressionManager).Split(SpecialCharacters.messageSeparator);
            for (int i = 0; i < messages.Length - 1; i++)
            {
                string message = messages[i];
                THIS.logger.LogWrite($"Message Received: {message}");
                Debug.WriteLine($"Message Received: {message}");
                string[] messageParts = message.Split(SpecialCharacters.messagePartSeparator);
                for (int j = 0; j < messageParts.Length; j++)
                {
                    if (messageParts[j].Length > 0 && messageParts[j][messageParts[j].Length - 1] == 'Â')
                    {
                        messageParts[j] = messageParts[j].Remove(messageParts[j].Length - 1);
                    }
                }
                if (messageParts.Length < 2) continue;
                if (messageParts[1] == "Rollback" || messageParts[1] == "DataUpdated" || messageParts[1] == "ItemCreated" || messageParts[1] == "ItemDeleted" || messageParts[1] == "CPLocations"
                    || messageParts[1] == "RequestRole" || messageParts[1] == "RemoveRole" || messageParts[1] == "ChannelCreated" || messageParts[1] == "ChannelModified")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.modelChangesManager.ProcessCommand(message, current);
                    }));
                }
                if (messageParts[1] == "Update")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.SendUpdate(current, Int32.Parse(messageParts[2]), Int64.Parse(messageParts[0]));
                    }));
                }
                if (messageParts[1] == "RequestID")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.GiveNewClientID(current, messageParts[2], messageParts[3], Int32.Parse(messageParts[4]));
                    }));
                }
                if (messageParts[1] == "RequestIDExisting")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.ExistingClientID(current, messageParts[2], messageParts[3], Int32.Parse(messageParts[4]));
                    }));
                }
                if (messageParts[1] == "ClientConnected")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.NewClientConnected(Int32.Parse(messageParts[2]), current);
                    }));
                }
            }
            try
            {
                current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                Debug.WriteLine("Second Socket Exception");
                lock (ServerCommunicator.socketsLock)
                {
                    current.Close();
                    foreach (var item in clientSockets.Where(x => x.Value == current).ToList())
                    {
                        clientSockets.Remove(item.Key);
                    }
                }
                return;
            }
        }

        /// <summary>
        /// Initialize server communicator (update information on remote server, start listenning to clients, set up message sending...)
        /// </summary>
        /// <param name="name">Name of the server</param>
        /// <param name="IP"></param>
        /// <param name="localPort">port the LARP server listens on</param>
        /// <param name="distantPort">Port the ngrok listens on</param>
        /// <param name="password">Server password (clients use it to access the server)</param>
        /// <param name="adminPassword">Administrator password</param>
        /// <param name="nick">Personal nick/name</param>
        /// <param name="newServer">New or existing server</param>
        /// <exception cref="WrongNameFormatException"></exception>
        /// <exception cref="WrongPortException"></exception>
        /// <exception cref="NotAnIPAddressException"></exception>
        /// <exception cref="CantConnectToCentralServerException"></exception>
        /// <exception cref="CantConnectToDatabaseException"></exception>
        /// <exception cref="WrongCredentialsException"></exception>
        public void initServerCommunicator(string name, string IP, int localPort, int distantPort, string password, string adminPassword, string nick, bool newServer)
        {
            CommunicationInfo.Instance.Communicator = this;
            CompressionManager = new Compression();
            Encryption.SetAESKey(password + name + "abcdefghijklmnopqrstu123456789qwertzuiop");
            logger = new DebugLogger(false);
            HttpClient client = new HttpClient();
            Regex nameRegex = new Regex(@"^[\w\s_\-]{1,50}$", RegexOptions.IgnoreCase);
            Debug.WriteLine("Created client, loaded dictionaries");
            if (!nameRegex.IsMatch(name))
            {
                throw new WrongNameFormatException("Name can only contain numbers, letters, spaces, - and _. It also must contain at most 50 characters");
            }
            if (localPort < 0 || localPort > 65535 || distantPort < 0 || distantPort > 65535)
            {
                throw new WrongPortException("Ports must be in range [0..65535]");
            }
            bool isIPValid = IPAddress.TryParse(IP, out _);
            if (!isIPValid)
            {
                throw new NotAnIPAddressException("Invalid IP address format");
            }
            Debug.WriteLine(Encryption.EncryptPassword(password));
            Debug.WriteLine(Encryption.EncryptPassword(adminPassword));
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + name + "\"" },
                { "IP", "\"" + IP + "\"" },
                { "port", distantPort.ToString() },
                { "password", "\"" + Encryption.EncryptPassword(password) + "\"" },
                { "adminPassword", "\"" + Encryption.EncryptPassword(adminPassword) + "\"" }
            };


            var content = new FormUrlEncodedContent(values);
            var responseString = "";
            if (newServer)
            {
                try
                {
                    var response = client.PostAsync("https://koblizekwebdesign.cz/LAMA/startserver.php", content);
                    responseString = response.Result.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine(responseString);
                    logger.LogWrite(responseString);
                }
                catch (HttpRequestException)
                {
                    Debug.WriteLine("Can not connect to the central server check your internet connection");
                    throw new CantConnectToCentralServerException("Nepodařilo se připojit k centrálnímu serveru, zkontrolujte si prosím vaše internetové připojení.");
                }


                if (responseString == "Connection")
                {
                    throw new CantConnectToDatabaseException("Nepodařilo se připojit k databázi.");
                }
                else if (responseString == "serverExists")
                {
                    throw new WrongCredentialsException("Server s tímto jménem už existuje, zvolte jiné jméno.");
                }
            }
            else
            {
                try
                {
                    var response = client.PostAsync("https://koblizekwebdesign.cz/LAMA/existingserver.php", content);
                    responseString = response.Result.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine(responseString);
                    logger.LogWrite(responseString);
                }
                catch (HttpRequestException)
                {
                    Debug.WriteLine("Can not connect to the central server check your internet connection");
                    throw new CantConnectToCentralServerException("Nepodařilo se připojit k centrálnímu serveru, zkontrolujte si prosím vaše internetové připojení.");
                }


                if (responseString == "Connection")
                {
                    throw new CantConnectToDatabaseException("Nepodařilo se připojit k databázi.");
                }
                else if (responseString == "credentials")
                {
                    throw new WrongCredentialsException("Neexistující server");
                }
                else if (responseString == "password")
                {
                    throw new WrongCredentialsException("password");
                }
            }

            Debug.WriteLine("No exceptions");

            CommunicationInfo.Instance.Communicator = this;
            CommunicationInfo.Instance.ServerName = name;
            CommunicationInfo.Instance.IsServer = true;

            //if (LarpEvent.Name != null && name != LarpEvent.Name) { Debug.WriteLine(LarpEvent.Name); SQLConnectionWrapper.ResetDatabase(); }
            LarpEvent.Name = name;
            attributesCache = DatabaseHolderStringDictionary<TimeValue, TimeValueStorage>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;

            IPAddress ipAddress;
            IPAddress.TryParse(IP, out ipAddress);
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localPort));
            }
            else
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Any, 42222));
                Debug.WriteLine("IPv4");
            }
            serverSocket.Listen(64);
            server = new Thread(StartServer);
            server.Start();
            THIS = this;
            Debug.WriteLine("Server started");
            modelChangesManager = new ModelChangesManager(this, objectsCache, attributesCache, true);

            Debug.WriteLine("Subscribing to events");
            SQLEvents.dataChanged += modelChangesManager.OnDataUpdated;
            SQLEvents.created += modelChangesManager.OnItemCreated;
            SQLEvents.dataDeleted += modelChangesManager.OnItemDeleted;

            LocalStorage.clientName = nick;
            LocalStorage.serverName = name;
            LocalStorage.cpID = 0;
            LocalStorage.clientID = 0;
            if (DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.getByID(0) == null)
            {
                //long cpID = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.nextID();
                //Debug.WriteLine($"cpID = {cpID}");
                DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.add(
                    new Models.CP(0,
                    nick, nick, new EventList<string> { "server", "org" }, "", "", "", ""));
            }
            //Server should have all permissions
            PermissionsManager.GiveAllPermissions();
            Debug.WriteLine("Initialization finished");
        }

        private bool checkNgrokAddressFormat(string address)
        {
            Regex regex = new Regex("tcp://.*\\.tcp\\..*\\.ngrok\\.io:[0-9]+", RegexOptions.IgnoreCase);
            return regex.IsMatch(address);
        }

        /// <summary>
        /// Create new ServerCommunicator - used to communicate with the clients
        /// </summary>
        /// <param name="name">Name of the server</param>
        /// <param name="IP"></param>
        /// <param name="port"></param>
        /// <param name="password">Server password</param>
        /// <param name="adminPassword">Server administrator password</param>
        /// <param name="nick">Personal nick</param>
        /// <param name="newServer">Is this a new or an existing server</param>
        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongCredentialsException">Wrong password used for existing server</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="WrongPortException">Port number not in the valid range</exception>
        /// <exception cref="PasswordTooShortException">The password is too short</exception>
        public ServerCommunicator(string name, string IP, int port, string password, string adminPassword, string nick, bool newServer)
        {
            if (password.Length < 5 || adminPassword.Length < 5) throw new PasswordTooShortException();
            initServerCommunicator(name, IP, port, port, password, adminPassword, nick, newServer);
        }

        /// <summary>
        /// Create new ServerCommunicator - used to communicate with the clients
        /// </summary>
        /// <param name="name">Name of the server</param>
        /// <param name="ngrokAddress"></param>
        /// <param name="password">Server password</param>
        /// <param name="adminPassword">Server administrator password</param>
        /// <param name="nick">Personal nick</param>
        /// <param name="newServer">Is this a new or an existing server</param>
        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongCredentialsException">Wrong password used for existing server</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="WrongPortException">Port number not in the valid range</exception>
        /// <exception cref="PasswordTooShortException">The password is too short</exception>
        /// <exception cref="WrongNgrokAddressFormatException">The ngrok endpoint supplied isn't in a correct format</exception>
        public ServerCommunicator(string name, string ngrokAddress, string password, string adminPassword, string nick, bool newServer)
        {
            if (!checkNgrokAddressFormat(ngrokAddress)) throw new WrongNgrokAddressFormatException();
            if (password.Length < 5 || adminPassword.Length < 5) throw new PasswordTooShortException();
            string[] addressParts = ngrokAddress.Split(':');
            IPAddress[] addresses = Dns.GetHostAddresses(addressParts[1].Trim('/'));
            Debug.WriteLine(addresses[0]);
            initServerCommunicator(name, addresses[0].ToString(), 42222, Int32.Parse(addressParts[2]), password, adminPassword, nick, newServer);
        }

        /// <summary>
        /// Insert command to the command queue (commands from this queue are sent automatically)
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="objectID"></param>
        private void SendCommand(string commandText, string objectID)
        {
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Command command = new Command(commandText, time, objectID);
            lock (ServerCommunicator.commandsLock)
            {
                commandsToBroadcast.Enqueue(command);
            }
        }

        /// <summary>
        /// Insert command to the command queue (commands from this queue are sent automatically)
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(Command command)
        {
            lock (ServerCommunicator.commandsLock)
            {
                commandsToBroadcast.Enqueue(command);
            }
        }

        /// <summary>
        /// Give ID to an existing client (the CP was already created)
        /// </summary>
        /// <param name="current"></param>
        /// <param name="clientName"></param>
        /// <param name="password"></param>
        /// <param name="clientID"></param>
        private void ExistingClientID(Socket current, string clientName, string password, int clientID)
        {
            bool passwordCorrect = true;
            if (clientID == -1)
            {
                LocalStorage.MaxClientID += 1;
                clientID = LocalStorage.MaxClientID;
            }
            long cpID = -1;
            for (int i = 0; i < DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count; i++)
            {
                //Add password testing
                if (DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i] != null &&
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].name == clientName)
                {
                    passwordCorrect = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].password == Encryption.EncryptPassword(password);
                    cpID = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].ID;
                    string command = $"GiveID{SpecialCharacters.messagePartSeparator}{clientID}{SpecialCharacters.messagePartSeparator}{cpID}";
                    lock (ServerCommunicator.socketsLock)
                    {
                        clientSockets[clientID] = current;
                    }
                    SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
                    SendCommand(new Command("Connected", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
                    return;
                }
            }
            lock (ServerCommunicator.socketsLock)
            {
                clientSockets[clientID] = current;
            }
            if (passwordCorrect)
                SendCommand(new Command($"ClientRefused{SpecialCharacters.messagePartSeparator}{clientID}{SpecialCharacters.messagePartSeparator}Client", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
            else
                SendCommand(new Command($"ClientRefused{SpecialCharacters.messagePartSeparator}{clientID}{SpecialCharacters.messagePartSeparator}Password", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
        }

        /// <summary>
        /// Give client and CP ID
        /// </summary>
        /// <param name="current"></param>
        /// <param name="clientName"></param>
        /// <param name="password"></param>
        /// <param name="clientID"></param>
        private void GiveNewClientID(Socket current, string clientName, string password, int clientID)
        {
            Debug.WriteLine("GiveNewClientID");
            if (clientID == -1)
            {
                LocalStorage.MaxClientID += 1;
                clientID = LocalStorage.MaxClientID;
            }
            long cpID = -1;
            for (int i = 0; i < DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count; i++)
            {
                if (DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i] != null &&
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].name == clientName)
                {
                    cpID = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].ID;
                    SendCommand(new Command($"ClientRefused{SpecialCharacters.messagePartSeparator}{clientID}{SpecialCharacters.messagePartSeparator}Client with this name already exists", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
                    lock (ServerCommunicator.socketsLock)
                    {
                        clientSockets[clientID] = current;
                    }
                    return;
                }
            }
            if (cpID == -1)
            {
                CP cp = new Models.CP(DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.nextID(),
                    clientName, clientName, new EventList<string> { }, "", "", "", "");
                DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.add(cp);
                cpID = cp.ID;
                cp.password = Encryption.EncryptPassword(password);
            }
            string command = $"GiveID{SpecialCharacters.messagePartSeparator}{clientID}{SpecialCharacters.messagePartSeparator}{cpID}";
            lock (ServerCommunicator.socketsLock)
            {
                clientSockets[clientID] = current;
            }
            SendCommand(new Command(command, DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
            SendCommand(new Command("Connected", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
        }

        /// <summary>
        /// Known client connected (reconnection)
        /// </summary>
        /// <param name="id">ID of the connected client</param>
        /// <param name="current"></param>
        private void NewClientConnected(int id, Socket current)
        {
            logger.LogWrite($"New Client Connected: {id}");
            lock (ServerCommunicator.socketsLock)
            {
                clientSockets[id] = current;
            }
            SendCommand(new Command("Connected", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", id));
        }

        /// <summary>
        /// Send information to a client about all the changes since a certain time
        /// </summary>
        /// <param name="current"></param>
        /// <param name="id">Client id</param>
        /// <param name="lastUpdateTime">The last time this client was updated</param>
        public void SendUpdate(Socket current, int id, long lastUpdateTime)
        {
            logger.LogWrite($"Sending Update: {id} | {lastUpdateTime}");
            for (int i = 0; i < objectsCache.Count; i++)
            {
                Command entry = objectsCache[i];
                if (entry.time > lastUpdateTime)
                {
                    SendCommand(new Command(entry.command, entry.time, entry.getKey(), id));
                }
            }

            for (int i = 0; i < attributesCache.Count; i++)
            {
                TimeValue entry = attributesCache[i];
                if (entry.time > lastUpdateTime)
                {
                    string value = entry.value;
                    string[] keyParts = entry.key.Split(SpecialCharacters.messagePartSeparator);
                    string command = "DataUpdated" + SpecialCharacters.messagePartSeparator + keyParts[0] + SpecialCharacters.messagePartSeparator + keyParts[1] + SpecialCharacters.messagePartSeparator + keyParts[2] + SpecialCharacters.messagePartSeparator + value;
                    SendCommand(new Command(command, entry.time, entry.key, id));
                }
            }
            SendCommand(new Command("UpdateFinished", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "update", id));
        }

        /// <summary>
        /// Send information to a client about all the changes since a certain time
        /// </summary>
        /// <param name="current"></param>
        /// <param name="id">Client id</param>
        /// <param name="lastUpdateTime">The last time this client was updated</param>
        public void SendUpdateAlt(Socket current, int id, long lastUpdateTime)
        {
            logger.LogWrite($"Sending Update: {id} | {lastUpdateTime}");
            for (int i = 0; i < objectsCache.Count; i++)
            {
                Command entry = objectsCache[i];
                string[] keyParts = entry.key.Split(SpecialCharacters.messageSeparator);
                string objectType = keyParts[0];
                long ID = Int64.Parse(keyParts[1]);
                if (entry.time > lastUpdateTime)
                {
                    Serializable serializable;
                    if (objectType == "LAMA.Models.LarpActivity") serializable = DatabaseHolder<LarpActivity, LarpActivityStorage>.Instance.rememberedList.getByID(ID);
                    else if (objectType == "LAMA.Models.CP") serializable = DatabaseHolder<CP, CPStorage>.Instance.rememberedList.getByID(ID);
                    else if (objectType == "LAMA.Models.InventoryItem") serializable = DatabaseHolder<InventoryItem, InventoryItemStorage>.Instance.rememberedList.getByID(ID);
                    else if (objectType == "LAMA.Models.ChatMessage") serializable = DatabaseHolder<ChatMessage, ChatMessageStorage>.Instance.rememberedList.getByID(ID);
                    else if (objectType == "LAMA.Models.EncyclopedyCategory") serializable = DatabaseHolder<EncyclopedyCategory, EncyclopedyCategoryStorage>.Instance.rememberedList.getByID(ID);
                    else if (objectType == "LAMA.Models.EncyclopedyRecord") serializable = DatabaseHolder<EncyclopedyRecord, EncyclopedyRecordStorage>.Instance.rememberedList.getByID(ID);
                    else continue;
                    string[] attributes = serializable.getAttributes();
                    string command = "ItemCreated" + SpecialCharacters.messageSeparator.ToString() + objectType + SpecialCharacters.messagePartSeparator.ToString() + String.Join(SpecialCharacters.attributesSeparator.ToString(), attributes);
                    SendCommand(new Command(command, entry.time, entry.getKey(), id));
                }
            }

            for (int i = 0; i < attributesCache.Count; i++)
            {
                TimeValue entry = attributesCache[i];
                string[] keyParts = entry.key.Split(SpecialCharacters.messageSeparator);
                Command creationEntry = objectsCache.getByKey(keyParts[0] + SpecialCharacters.messageSeparator.ToString() + keyParts[1]);
                if (entry.time > lastUpdateTime && entry.time > creationEntry.time && !(creationEntry.time > lastUpdateTime))
                {
                    string value = entry.value;
                    string command = "DataUpdated" + SpecialCharacters.messageSeparator.ToString() + keyParts[0] + SpecialCharacters.messageSeparator.ToString() + keyParts[1] + SpecialCharacters.messageSeparator.ToString() + keyParts[2] + SpecialCharacters.messageSeparator.ToString() + value;
                    SendCommand(new Command(command, entry.time, entry.key, id));
                }
            }
            SendCommand(new Command("UpdateFinished", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "update", id));
        }
    }
}