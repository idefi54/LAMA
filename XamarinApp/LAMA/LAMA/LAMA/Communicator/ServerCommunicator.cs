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

        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;

        private static object socketsLock = new object();
        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();

        private static object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();
        private Command currentCommand = null;

        private ModelChangesManager modelChangesManager;

        private int maxClientID;
        /// <summary>
        /// Start accepting clients and broadcasting messages to them
        /// </summary>
        private async void StartServer()
        {
            logger.LogWrite("Starting Server");
            Debug.WriteLine("Starting Server");
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            logger.LogWrite("BeginAccept");
            Debug.WriteLine("BeginAccept");

            timer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

            CancellationToken token = new CancellationToken();
            await SendLocations(token);
        }

        public async Task SendLocations(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(30_000);
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
                    byte[] data = currentCommand.Encode();
                    Debug.WriteLine($"{Encryption.DecryptStringFromBytes_Aes(data)}");
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
                        else
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
            Debug.WriteLine($"{Encryption.DecryptStringFromBytes_Aes(data)}");
            string[] messages = Encryption.DecryptStringFromBytes_Aes(data).Split('|');

            for (int i = 0; i < messages.Length - 1; i++)
            {
                string message = messages[i];
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
                if (messageParts[1] == "Rollback" || messageParts[1] == "DataUpdated" || messageParts[1] == "ItemCreated" || messageParts[1] == "ItemDeleted" || messageParts[1] == "CPLocations")
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
                if (messageParts[1] == "GiveID")
                {
                    Device.BeginInvokeOnMainThread(new Action(() =>
                    {
                        THIS.GiveNewClientID(current, messageParts[2], messageParts[3], Int32.Parse(messageParts[4]));
                    }));
                }
                if (messageParts[1] == "GiveIDExisting")
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
        /// <exception cref="WrongCreadintialsException"></exception>
        public void initServerCommunicator(string name, string IP, int localPort, int distantPort, string password, string adminPassword, string nick, bool newServer)
        {
            if (name != LarpEvent.Name && LarpEvent.Name != null) { Debug.WriteLine(LarpEvent.Name); SQLConnectionWrapper.ResetDatabase(); }
            logger = new DebugLogger(false);
            Debug.WriteLine("After LarpEvent.Name test");
            LarpEvent.Name = name;
            attributesCache = DatabaseHolderStringDictionary<TimeValue, TimeValueStorage>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
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
                    throw new WrongCreadintialsException("Server s tímto jménem už existuje, zvolte jiné jméno.");
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
                else if (responseString == "credintials")
                {
                    throw new WrongCreadintialsException("Špatné heslo, nebo neexistující server.");
                }
                else if (responseString == "password")
                {
                    throw new WrongCreadintialsException("password.");
                }
            }
            Debug.WriteLine("No exceptions");
            Encryption.SetAESKey(password + name + "abcdefghijklmnopqrstu123456789qwertzuiop");
            //Debug.WriteLine(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes("Testovací český string žščřť")) + "\n");
            byte[] encrypted = Encryption.EncryptStringToBytes_Aes("ItemCreated;LAMA.Models.ChatMessage;2675274417265¦Klient¦0¦Hello¦1675274417265");
            //byte[] encrypted = Encoding.UTF8.GetBytes(Encryption.EncryptAES("Testovací český string žščřť"));
            Debug.WriteLine($"Decrypted AES: {Encryption.DecryptStringFromBytes_Aes(encrypted)} \n");

            //maxClientID = 0;
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
            /*
            //Initialize Intervals
            DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestCP;
            DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestInventoryItem;
            DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestLarpActivity;
            DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestChatMessage;

            Debug.WriteLine("-------------------------------------0------------------------------------------");
            DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.InvokeGiveNewInterval();
            Debug.WriteLine("-------------------------------------1------------------------------------------");
            DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.InvokeGiveNewInterval();
            Debug.WriteLine("-------------------------------------2------------------------------------------");
            DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.InvokeGiveNewInterval();
            Debug.WriteLine("-------------------------------------3------------------------------------------");
            DatabaseHolder<Models.ChatMessage, Models.ChatMessageStorage>.Instance.rememberedList.InvokeGiveNewInterval();
            Debug.WriteLine("-------------------------------------4------------------------------------------");
            */
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
            Debug.WriteLine("Initialization finished");
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
        /// <exception cref="WrongCreadintialsException">Wrong password used for existing server</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="WrongPortException">Port number not in the valid range</exception>
        public ServerCommunicator(string name, string IP, int port, string password, string adminPassword, string nick, bool newServer)
        {
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
        /// <exception cref="WrongCreadintialsException">Wrong password used for existing server</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="WrongPortException">Port number not in the valid range</exception>
        public ServerCommunicator(string name, string ngrokAddress, string password, string adminPassword, string nick, bool newServer)
        {
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
            logger.LogWrite($"Sending Command: {commandText} | {time} | {objectID}");
            Debug.WriteLine($"Sending Command: {commandText} | {time} | {objectID}");
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
            logger.LogWrite($"Sending Command: {command.command} | {command.time} | {command.key}");
            Debug.WriteLine($"Sending Command: {command.command} | {command.time} | {command.key}");
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
            if (clientID == -1)
            {
                maxClientID += 1;
                clientID = maxClientID;
            }
            long cpID = -1;
            for (int i = 0; i < DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count; i++)
            {
                //Add password testing
                if (DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i] != null &&
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].name == clientName)
                {
                    cpID = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].ID;
                    string command = $"GiveID;{clientID};{cpID}";
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
            SendCommand(new Command($"ClientRefused;{clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
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
            if (clientID == -1)
            {
                maxClientID += 1;
                clientID = maxClientID;
            }
            long cpID = -1;
            for (int i = 0; i < DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.Count; i++)
            {
                if (DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i] != null &&
                    DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].name == clientName)
                {
                    cpID = DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList[i].ID;
                    SendCommand(new Command($"ClientRefused;{clientID}", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "None", clientID));
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
                    clientName, clientName, new EventList<string> {}, "", "", "", "");
                DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.add(cp);
                cpID = cp.ID;
            }
            string command = $"GiveID;{clientID};{cpID}";
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
                    string[] keyParts = entry.key.Split(';');
                    string command = "DataUpdated" + ";" + keyParts[0] + ";" + keyParts[1] + ";" + keyParts[2] + ";" + value;
                    SendCommand(new Command(command, entry.time, entry.key, id));
                }
            }
        }
    }
}