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
            get { return DateTimeOffset.Now.ToUnixTimeSeconds(); }
            set { }
        }

        static byte[] buffer = new byte[8 * 1024];
        static Socket serverSocket;
        private Thread server;
        private static ServerCommunicator THIS;

        private RememberedStringDictionary<TimeValue, TimeValueStorage> attributesCache;
        private RememberedStringDictionary<Command, CommandStorage> objectsCache;

        private object socketsLock = new object();
        private static Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();

        private object commandsLock = new object();
        private Queue<Command> commandsToBroadcast = new Queue<Command>();
        private Command currentCommand = null;

        private ModelChangesManager modelChangesManager;
        private IntervalCommunicationManagerServer intervalsManager;

        private int maxClientID;

        private void StartServer()
        {
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            var timer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
        }

        private void ProcessBroadcast()
        {
            while (true)
            {
                lock (commandsLock)
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
                    byte[] data = currentCommand.Encode();
                    List<int> socketsToRemove = new List<int>();
                    lock (socketsLock)
                    {
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
                                        client.Close();
                                        socketsToRemove.Add(entry.Key);
                                    }
                                }
                                else
                                {
                                    client.Close();
                                    socketsToRemove.Add(entry.Key);
                                }
                            }
                        }
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
                                    client.Close();
                                    socketsToRemove.Add(currentCommand.receiverID);
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
                            clientSockets.Remove(i);
                        }
                    }
                    logger.LogWrite($"Finished Sending: {currentCommand.command}");
                }
            }
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            try
            {
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), socket);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (THIS.socketsLock)
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

        private static void ReceiveData(IAsyncResult AR)
        {
            int received;
            Socket current = (Socket)AR.AsyncState;
            try
            {
                received = current.EndReceive(AR);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (THIS.socketsLock)
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
            string message = Encoding.Default.GetString(data);
            THIS.logger.LogWrite($"Message Received: {message}");
            string[] messageParts = message.Split(';');
            if (messageParts[1] == "DataUpdated")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.modelChangesManager.DataUpdated(messageParts[2], Int32.Parse(messageParts[3]), Int32.Parse(messageParts[4]), messageParts[5], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1), current);
                }));
            }
            if (messageParts[1] == "ItemCreated")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.modelChangesManager.ItemCreated(messageParts[2], messageParts[3], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1), current);
                }));
            }
            if (messageParts[1] == "ItemDeleted")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.modelChangesManager.ItemDeleted(messageParts[2], Int32.Parse(messageParts[3]), Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "Update")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.SendUpdate(current, Int32.Parse(messageParts[2]), Int64.Parse(messageParts[0]));
                }));
            }
            if (messageParts[1] == "Interval")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.intervalsManager.IntervalsUpdate(messageParts[2], messageParts[3], Int32.Parse(messageParts[4]), Int32.Parse(messageParts[5]), Int32.Parse(messageParts[6]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "GiveID")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.GiveNewClientID(current);
                }));
            }
            if (messageParts[1] == "ClientConnected")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.NewClientConnected(Int32.Parse(messageParts[2]), current);
                }));
            }
            try {
                current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (THIS.socketsLock)
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

        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongPasswordException">Wrong password used for existing server</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="WrongPortException">Port number not in the valid range</exception>
        public ServerCommunicator(string name, string IP, int port, string password)
        {
            Debug.WriteLine("Server communicator created");
            logger = new DebugLogger(false);
            attributesCache = DatabaseHolderStringDictionary<TimeValue, TimeValueStorage>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
            HttpClient client = new HttpClient();
            Regex nameRegex = new Regex(@"^[\w\s_\-]{1,50}$", RegexOptions.IgnoreCase);
            logger.LogWrite("Created client, loaded dictionaries");
            if (!nameRegex.IsMatch(name))
            {
                throw new WrongNameFormatException("Name can only contain numbers, letters, spaces, - and _. It also must contain at most 50 characters");
            }
            if (port < 0 || port > 65535)
            {
                throw new WrongPortException("Ports must be in range [0..65535]");
            }
            bool isIPValid = IPAddress.TryParse(IP, out _);
            if (!isIPValid)
            {
                throw new NotAnIPAddressException("Invalid IP address format");
            }
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + name + "\"" },
                { "IP", "\"" + IP + "\"" },
                { "port", port.ToString() },
                { "password", "\"" + password + "\"" }
            };


            var content = new FormUrlEncodedContent(values);
            var responseString = "";
            try
            {
                var response = client.PostAsync("https://larp-project-mff.000webhostapp.com/startserver.php", content);
                responseString = response.Result.Content.ReadAsStringAsync().Result;
                logger.LogWrite(responseString);
            }
            catch (HttpRequestException)
            {
                throw new CantConnectToCentralServerException("Can not connect to the central server check your internet connection");
            }


            if (responseString == "Connection")
            {
                throw new CantConnectToDatabaseException("Connecting to database failed");
            }
            else if (responseString == "password")
            {
                throw new WrongPasswordException("Wrong password for existing server");
            }

            logger.LogWrite("No exceptions");
            maxClientID = 0;
            serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            serverSocket.Listen(64);
            server = new Thread(StartServer);
            server.Start();
            THIS = this;
            logger.LogWrite("Server started");
            modelChangesManager = new ModelChangesManager(this, objectsCache, attributesCache, true);
            intervalsManager = new IntervalCommunicationManagerServer(this);
            logger.LogWrite("Subscribing to events");
            SQLEvents.dataChanged += modelChangesManager.OnDataUpdated;
            SQLEvents.created += modelChangesManager.OnItemCreated;
            SQLEvents.dataDeleted += modelChangesManager.OnItemDeleted;
            DatabaseHolder<Models.CP, Models.CPStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestCP;
            DatabaseHolder<Models.InventoryItem, Models.InventoryItemStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestInventoryItem;
            DatabaseHolder<Models.LarpActivity, Models.LarpActivityStorage>.Instance.rememberedList.GiveNewInterval += intervalsManager.OnIntervalRequestLarpActivity;
            logger.LogWrite("Initialization finished");
        }

        private void SendCommand(string commandText, string objectID)
        {
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Command command = new Command(commandText, time, objectID);
            logger.LogWrite($"Sending Command: {commandText} | {time} | {objectID}");
            lock (commandsLock)
            {
                commandsToBroadcast.Enqueue(command);
            }
        }

        public void SendCommand(Command command)
        {
            logger.LogWrite($"Sending Command: {command.command} | {command.time} | {command.key}");
            lock (commandsLock)
            {
                commandsToBroadcast.Enqueue(command);
            }
        }

        private void GiveNewClientID(Socket current)
        {
            maxClientID += 1;
            string command = "GiveID;";
            command += maxClientID;
            lock (THIS.socketsLock)
            {
                clientSockets[maxClientID] = current;
            }
            SendCommand(new Command(command, "None", maxClientID));
            SendCommand(new Command("Connected", "None", maxClientID));
        }

        private void NewClientConnected(int id, Socket current)
        {
            logger.LogWrite($"New Client Connected: {id}");
            lock (THIS.socketsLock)
            {
                clientSockets[id] = current;
            }
            SendCommand(new Command("Connected", "None", id));
        }

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
