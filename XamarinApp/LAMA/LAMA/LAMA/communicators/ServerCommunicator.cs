using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Essentials;

namespace LAMA.Communicator
{
    public class ServerCommunicator
    {
        static byte[] buffer = new byte[8 * 1024];
        static Socket serverSocket;
        private Thread server;
        private static ServerCommunicator THIS;

        private RememberedStringDictionary<TimeValue> attributesCache;
        private RememberedStringDictionary<Command> objectsCache;

        private object socketsLock = new object();
        private static List<Socket> clientSockets = new List<Socket>();

        private object commandsLock = new object();
        private Queue<Command> commandsToBroadCast = new Queue<Command>();
        private Command currentCommand = null;

        private void StartServer()
        {
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            var timer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        }

        private void ProcessBroadcast()
        {
            lock (commandsLock)
            {
                if (commandsToBroadCast.Count > 0)
                {
                    currentCommand = commandsToBroadCast.Dequeue();
                }
                else
                {
                    currentCommand = null;
                }
            }
            if (currentCommand != null)
            {
                byte[] data = currentCommand.Encode();
                foreach (var client in clientSockets)
                {
                    if (client.Connected)
                    {
                        try
                        {
                            client.Send(data);
                        }
                        catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                        {
                            lock (socketsLock)
                            {
                                client.Close();
                                clientSockets.Remove(client);
                            }
                        }
                    }
                    else
                    {
                        lock (socketsLock)
                        {
                            client.Close();
                            clientSockets.Remove(client);
                        }
                    }
                }
            }
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            lock (THIS.socketsLock)
            {
                clientSockets.Add(socket);
            }
            try
            {
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), socket);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (THIS.socketsLock)
                {
                    socket.Close();
                    clientSockets.Remove(socket);
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
                    clientSockets.Remove(current);
                }
                return;
            }
            byte[] data = new byte[received];
            Array.Copy(buffer, data, received);
            string message = Encoding.Default.GetString(data);
            string[] messageParts = message.Split(';');
            if (messageParts[1] == "DataUpdated")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.DataUpdated(messageParts[2], Int32.Parse(messageParts[3]), Int32.Parse(messageParts[4]), messageParts[5], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "ItemCreated")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.ItemCreated(messageParts[2], messageParts[3], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "ItemDeleted")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.ItemDeleted(messageParts[2], Int32.Parse(messageParts[3]), Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "Update")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.SendUpdate(current, Int64.Parse(messageParts[0]));
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
                    clientSockets.Remove(current);
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
            attributesCache = DatabaseHolderStringDictionary<TimeValue>.Instance.rememberedDictionary;
            objectsCache = DatabaseHolderStringDictionary<Command>.Instance.rememberedDictionary;
            HttpClient client = new HttpClient();
            Regex nameRegex = new Regex(@"^[\w\s_\-]{1,50}$", RegexOptions.IgnoreCase);
            if (!nameRegex.IsMatch(name))
            {
                throw new WrongNameFormatException("Name can only contain numbers, letters, spaces, - and _. It also must contain at most 50 charcters");
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

            serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            serverSocket.Listen(64);
            server = new Thread(StartServer);
            server.Start();
            THIS = this;
            SQLEvents.dataChanged += OnDataUpdated;
            SQLEvents.created += OnItemCreated;
            SQLEvents.dataDeleted += OnItemDeleted;
        }

        private void SendCommand(string commandText, string objectID)
        {
            Command command = new Command(commandText, DateTimeOffset.Now.ToUnixTimeMilliseconds(), objectID);
            lock (commandsLock)
            {
                commandsToBroadCast.Enqueue(command);
            }
        }

        private void SendCommand(Command command)
        {
            lock (commandsLock)
            {
                commandsToBroadCast.Enqueue(command);
            }
        }

        public void OnDataUpdated(Serializable changed, int attributeIndex)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string attributeID = objectType + ";" + objectID + ";" + attributeIndex;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            attributesCache.getByKey(attributeID).value = changed.getAttribute(attributeIndex);
            attributesCache.getByKey(attributeID).time = updateTime;
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + changed.getAttribute(attributeIndex);
            THIS.SendCommand(new Command(command, updateTime, objectType + ";" + objectID));
        }

        public void DataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.containsKey(attributeID) && attributesCache.getByKey(attributeID).time <= updateTime)
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

                /*
                 * TO DO - update game data (once there is a way for me to access it)
                 */
                if (objectType == "LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "CP")
                {
                    DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }
            }
            // Notify every CP
            THIS.SendCommand(new Command(command, updateTime, objectType + ";" + objectID));
        }

        public void OnItemCreated(Serializable changed)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemCreated" + ";" + objectType + ";" + String.Join(",", attributes);

            if (!objectsCache.containsKey(objectCacheID))
            {
                objectsCache.add(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.add(new TimeValue(updateTime, attributes[i], objectID + ";" + i));
                }
                // Notify every CP of item creation
                THIS.SendCommand(new Command(command, updateTime, objectCacheID));
            }
        }

        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            if (objectType == "LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    /*
                     * TO DO - update game data once there is access to it
                     */
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.add(activity, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every CP of item creation
                    THIS.SendCommand(new Command(command, updateTime, objectID));
                }
                else
                {
                    /*
                     * TO DO - notify sender of unsuccessful creation
                     */
                }
            }

            else if (objectType == "CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.CP>.Instance.rememberedList.add(cp, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every CP of item creation
                    THIS.SendCommand(new Command(command, updateTime, objectID));
                }
                else
                {
                    /*
                     * TO DO - notify sender of unsuccessful creation
                     */
                }
            }

            else if (objectType == "InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.add(ii, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                    // Notify every CP of item creation
                    THIS.SendCommand(new Command(command, updateTime, objectID));
                }
                else
                {
                    /*
                     * TO DO - notify sender of unsuccessful creation
                     */
                }
            }
        }

        public void OnItemDeleted(Serializable changed)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string objectCacheID = objectType + ";" + objectID;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            string[] attributes = changed.getAttributes();
            string command = "ItemDeleted" + ";" + objectType + ";" + objectID;

            if (objectsCache.containsKey(objectCacheID))
            {
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;
                // Notify every CP of item deletion
                THIS.SendCommand(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
            }
        }

        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.containsKey(objectCacheID))
            {
                /*
                 * TO DO - Find object and its attributes
                 * Delete attributes from cache
                 * Delete object
                 */
                Models.LarpActivity removedItemActivity;
                if (objectType == "LarpActivity" && (removedItemActivity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    int nAttributes = removedItemActivity.numOfAttributes();
                    for (int i = 0; i < nAttributes; i++)
                    {
                        attributesCache.removeByKey(objectID + ";" + i);
                    }
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.removeByID(objectID);
                }

                Models.CP removedItemCP;
                if (objectType == "CP" && (removedItemCP = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    int nAttributes = removedItemCP.numOfAttributes();
                    for (int i = 0; i < nAttributes; i++)
                    {
                        attributesCache.removeByKey(objectID + ";" + i);
                    }
                    DatabaseHolder<Models.CP>.Instance.rememberedList.removeByID(objectID);
                }

                Models.InventoryItem removedItemInventoryItem;
                if (objectType == "InventoryItem" && (removedItemInventoryItem = DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    int nAttributes = removedItemInventoryItem.numOfAttributes();
                    for (int i = 0; i < nAttributes; i++)
                    {
                        attributesCache.removeByKey(objectID + ";" + i);
                    }
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.removeByID(objectID);
                }
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;

                // Notify every CP of deletion
                THIS.SendCommand(new Command(command, updateTime, objectCacheID));
            }
        }

        public void SendUpdate(Socket current, long lastUpdateTime)
        {
            for (int i = 0; i < objectsCache.Count; i++)
            {
                Command entry = objectsCache[i];
                if (entry.time > lastUpdateTime)
                {
                    try
                    {
                        current.Send(entry.Encode());
                    }
                    catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                    {
                        return;
                    }
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
                    try
                    {
                        current.Send((new Command(command, entry.time, keyParts[0] + ";" + keyParts[1])).Encode());
                    }
                    catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                    {
                        return;
                    }
                }
            }
        }
    }
}
