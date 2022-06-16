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

namespace LAMA.Communicator
{
    public class ClientCommunicator
    {
        static Socket s;
        private object socketLock = new object();
        private Thread listener;
        private long lastUpdate;
        private static string CPName;
        private static string assignedEvent = "";
        private IPAddress _IP;
        private int _port;
        private bool _IPv6 = false;
        private bool _IPv4 = false;

        static byte[] buffer = new byte[8 * 1024];
        private static ClientCommunicator THIS;
        private RememberedStringDictionary<Command> objectsCache;
        private RememberedStringDictionary<TimeValue> attributesCache;
        static Random rd = new Random();

        private object commandsLock = new object();
        private Queue<Command> commandsToBroadCast = new Queue<Command>();

        private int id;
        private void ProcessBroadcast()
        {
            if (s.Connected)
            {
                lock (commandsLock)
                {
                    while (commandsToBroadCast.Count > 0)
                    {
                        Command currentCommand = commandsToBroadCast.Dequeue();
                        byte[] data = currentCommand.Encode();
                        lock (socketLock)
                        {
                            try
                            {
                                s.Send(data);
                            }
                            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                            {
                                s.Close();
                            }
                        }
                    }
                }
            }
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        private void SendCommand(Command command)
        {
            lock (commandsLock)
            {
                commandsToBroadCast.Enqueue(command);
            }
        }

        private static void ReceiveData(IAsyncResult AR)
        {
            int received;
            Socket current = (Socket)AR.AsyncState;
            lock (THIS.socketLock)
            {
                try
                {
                    received = current.EndReceive(AR);
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    current.Close();
                    return;
                }
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
            if (messageParts[1] == "Interval")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.IntervalsUpdate(messageParts[2], messageParts[3], Int32.Parse(messageParts[4]), Int32.Parse(messageParts[5]), Int32.Parse(messageParts[6]), message.Substring(message.IndexOf(';') + 1));
                }));
            }
            if (messageParts[1] == "GiveID")
            {
                MainThread.BeginInvokeOnMainThread(new Action(() =>
                {
                    THIS.ReceiveID(Int32.Parse(messageParts[2]));
                }));
            }
            if (messageParts[1] == "Rollback")
            {
                if (messageParts[2] == "DataUpdated")
                {
                    THIS.RollbackDataUpdated(messageParts[3], Int32.Parse(messageParts[4]), Int32.Parse(messageParts[5]), messageParts[6], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }
                if (messageParts[2] == "ItemCreated")
                {
                    THIS.RollbackItemCreated(messageParts[3], messageParts[4], Int64.Parse(messageParts[0]), message.Substring(message.IndexOf(';') + 1));
                }
            }
            try
            {
                current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
            }
            catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
            {
                lock (THIS.socketLock)
                {
                    current.Close();
                }
                return;
            }
        }


        private void OnIntervalRequestCP()
        {
            OnIntervalRequest("CP");
        }

        private void OnIntervalRequestInventoryItem()
        {
            OnIntervalRequest("InventoryItem");
        }

        private void OnIntervalRequestLarpActivity()
        {
            OnIntervalRequest("LarpActivity");
        }

        private void OnIntervalRequest(string type)
        {
            string command = "Interval" + ";" + "Request" + ";" + type + ";" + 0 + ";" + 0 + ";" + id;
            SendCommand(new Command(command, "None"));
        }

        private void IntervalsUpdate(string intervalCommand, string objectType, int lowerLimit, int upperLimit, int id, string command)
        {
            if (intervalCommand == "Add" && id == this.id)
            {
                if (objectType == "LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.NewIntervalReceived(new Pair<int, int>(lowerLimit, upperLimit));
                }

                if (objectType == "CP")
                {
                    DatabaseHolder<Models.CP>.Instance.rememberedList.NewIntervalReceived(new Pair<int, int>(lowerLimit, upperLimit));
                }

                if (objectType == "InventoryItem")
                {
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.NewIntervalReceived(new Pair<int, int>(lowerLimit, upperLimit));
                }
            }
        }

        /*
        private void SendConnectionInfo()
        {
            string q = "ClientConnected;";
            q += CPName + ";";
            byte[] data = Encoding.Default.GetBytes(q);
            s.Send(data);
        }
        */

        private void ReceiveID(int id)
        {
            this.id = id;
        }

        private void RequestUpdate()
        {
            string q = lastUpdate.ToString() + ";";
            q += "Update;";
            q += CPName + ";";
            SendCommand(new Command(q, "None"));
        }

        private void StartListening()
        {
            RequestUpdate();
            var timer = new System.Threading.Timer((e) =>
            {
                ProcessBroadcast();
            }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
            lock (THIS.socketLock)
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

        private void InitSocket()
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

        private bool Connect()
        {
            if (!s.Connected)
            {
                try
                {
                    InitSocket();
                    s.Connect(_IP, _port);
                    SendCommand(new Command("GiveID", "None"));
                    listener = new Thread(StartListening);
                    listener.Start();
                }
                catch (SocketException e)
                {
                    if (e.Message == "Connection refused")
                    {
                        throw new ServerConnectionRefusedException("Server refused the connection, check your port forwarding and firewall settings");
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        /// <exception cref="CantConnectToCentralServerException">Can't connect to the central server</exception>
        /// <exception cref="CantConnectToDatabaseException">Connecting to database failed</exception>
        /// <exception cref="WrongPasswordException">Wrong password used</exception>
        /// <exception cref="NonExistentServerException">Server with this name doesn't exist</exception>
        /// <exception cref="NotAnIPAddressException">Invalid IP address format</exception>
        /// <exception cref="ServerConnectionRefusedException">The server refused your connection</exception>
        public ClientCommunicator(string serverName, string password, string clientName)
        {
            attributesCache = DatabaseHolderStringDictionary<TimeValue>.Instance.rememberedDictionary;
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + serverName + "\"" },
                { "password", "\"" + password + "\"" }
            };

            var content = new FormUrlEncodedContent(values);
            var responseString = "";
            try
            {
                var response = client.PostAsync("https://larp-project-mff.000webhostapp.com/findserver.php", content);
                responseString = response.Result.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException)
            {
                throw new CantConnectToCentralServerException("Can not connect to the server, check your internet connection");
            }

            if (responseString == "Connection")
            {
                throw new CantConnectToDatabaseException("Connecting to database failed");
            }
            else if (responseString == "password")
            {
                throw new WrongPasswordException("Wrong password");
            }
            else if (responseString == "server")
            {
                throw new NonExistentServerException("No such server exists");
            }
            else
            {
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
                    throw new NotAnIPAddressException("Server IP address not valid");
                }
                _port = int.Parse(array[1]);
                CPName = CreateString(10);
                InitSocket();
                Connect();
                var timer = new System.Threading.Timer((e) =>
                {
                    Connect();
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(2000));
                lastUpdate = DateTimeOffset.MinValue.ToUnixTimeMilliseconds();
                SQLEvents.dataChanged += OnDataUpdated;
                SQLEvents.created += OnItemCreated;
                SQLEvents.dataDeleted += OnItemDeleted;
                DatabaseHolder<Models.CP>.Instance.rememberedList.GiveNewInterval += OnIntervalRequestCP;
                DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.GiveNewInterval += OnIntervalRequestInventoryItem;
                DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.GiveNewInterval += OnIntervalRequestLarpActivity;
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
            lastUpdate = updateTime;
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.containsKey(attributeID) && attributesCache.getByKey(attributeID).time <= updateTime)
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

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
        }

        public void RollbackDataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command)
        {
            lastUpdate = updateTime;
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.containsKey(attributeID))
            {
                attributesCache.getByKey(attributeID).value = value;
                attributesCache.getByKey(attributeID).time = updateTime;

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
                THIS.SendCommand(new Command(command, updateTime, objectCacheID));
            }
        }

        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            lastUpdate = updateTime;
            if (objectType == "LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.containsKey(objectID))
                {
                    objectsCache.add(new Command(command, updateTime, objectID));
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.add(activity, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.add(new TimeValue(updateTime, attributtes[i], objectID + ";" + i));
                    }
                }
            }

            if (objectType == "CP")
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
                }
            }

            if (objectType == "InventoryItem")
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
                }
            }
        }

        public void RollbackItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            lastUpdate = updateTime;
            if (objectType == "LarpActivity")
            {
                Models.LarpActivity activity = new Models.LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();
                int activityID = activity.getID();

                if (objectsCache.containsKey(objectID))
                {

                    activity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(activityID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    activity.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }

            if (objectType == "CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                int cpID = cp.getID();

                if (objectsCache.containsKey(objectID))
                {

                    cp = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(cpID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    cp.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
                }
            }

            if (objectType == "InventoryItem")
            {
                Models.InventoryItem ii = new Models.InventoryItem();
                string[] attributtes = serializedObject.Split(',');
                ii.buildFromStrings(attributtes);
                string objectID = objectType + ";" + ii.getID();
                int itemID = ii.getID();

                if (objectsCache.containsKey(objectID))
                {

                    ii = DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(itemID);
                    objectsCache.getByKey(objectID).command = command;
                    objectsCache.getByKey(objectID).time = updateTime;
                    ii.buildFromStrings(attributtes);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache.getByKey(objectID + ";" + i).value = attributtes[i];
                        attributesCache.getByKey(objectID + ";" + i).time = updateTime;
                    }
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
                THIS.SendCommand(new Command(command, updateTime, objectCacheID));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
            }
        }

        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            lastUpdate = updateTime;
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.containsKey(objectCacheID))
            {
                int nAttributes = 0;
                Models.LarpActivity removedActivity;
                if (objectType == "LarpActivity" && (removedActivity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID)) != null) {
                    nAttributes = removedActivity.numOfAttributes();
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.removeByID(objectID);
                }
                Models.CP removedCP;
                if (objectType == "CP" && (removedCP = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedCP.numOfAttributes();
                    DatabaseHolder<Models.CP>.Instance.rememberedList.removeByID(objectID);
                }
                Models.InventoryItem removedInventoryItem;
                if (objectType == "InventoryItem" && (removedInventoryItem = DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedInventoryItem.numOfAttributes();
                    DatabaseHolder<Models.InventoryItem>.Instance.rememberedList.removeByID(objectID);
                }
                for (int i = 0; i < nAttributes; i++)
                {
                    attributesCache.removeByKey(objectID + ";" + i);
                }
                objectsCache.getByKey(objectCacheID).command = command;
                objectsCache.getByKey(objectCacheID).time = updateTime;
            }
        }
    }
}
