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
        public object socketLock = new object();
        public Thread listener;
        public long lastUpdate;
        private static string CPName;
        private static string assignedEvent = "";
        private IPAddress _IP;
        private int _port;
        private bool _IPv6 = false;
        private bool _IPv4 = false;

        static byte[] buffer = new byte[8 * 1024];
        public static ClientCommunicator THIS;
        public Dictionary<string, Command> objectsCache = new Dictionary<string, Command>();
        public Dictionary<string, TimeValue> attributesCache = new Dictionary<string, TimeValue>();
        static Random rd = new Random();

        public object commandsLock = new object();
        private Queue<Command> commandsToBroadCast = new Queue<Command>();

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
            lock (THIS.socketLock)
            {
                try
                {
                    current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    current.Close();
                    return;
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

        private void RequestUpdate()
        {
            string q = lastUpdate.ToString() + ";";
            q += "Update;";
            q += CPName + ";";
            SendCommand(new Command(q));
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
                    listener = new Thread(StartListening);
                    listener.Start();
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        /// <exception cref="HttpRequestException">Can't connect to the server</exception>
        public ClientCommunicator(string serverName, string password, string clientName)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + serverName + "\"" },
                { "password", "\"" + password + "\"" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync("https://larp-project-mff.000webhostapp.com/findserver.php", content);
            var responseString = response.Result.Content.ReadAsStringAsync().Result;

            if (responseString == "Connection")
            {
                throw new Exception("Connecting to database failed");
            }
            else if (responseString == "password")
            {
                throw new Exception("Wrong password");
            }
            else if (responseString == "server")
            {
                throw new Exception("No such server exists");
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
                    throw new Exception("Server IP address not valid");
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
            }
        }

        public void OnDataUpdated(Serializable changed, int attributeIndex)
        {
            int objectID = changed.getID();
            string objectType = changed.GetType().ToString();
            string attributeID = objectType + ";" + objectID + ";" + attributeIndex;

            long updateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            attributesCache[attributeID].value = changed.getAttribute(attributeIndex);
            attributesCache[attributeID].time = updateTime;
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + changed.getAttribute(attributeIndex);
            THIS.SendCommand(new Command(command, updateTime));
        }

        public void DataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command)
        {
            lastUpdate = updateTime;
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.ContainsKey(attributeID) && attributesCache[attributeID].time <= updateTime)
            {
                attributesCache[attributeID].value = value;
                attributesCache[attributeID].time = updateTime;

                if (objectType == "LarpActivity")
                {
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
                }

                if (objectType == "CP")
                {
                    DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID).setAttribute(indexAttribute, value);
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

            if (!objectsCache.ContainsKey(objectCacheID))
            {
                objectsCache[objectCacheID] = new Command(command, updateTime);
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache[objectID + ";" + i] = new TimeValue(updateTime, attributes[i]);
                }
                THIS.SendCommand(new Command(command, updateTime));
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

                if (!objectsCache.ContainsKey(objectID))
                {
                    objectsCache[objectID] = new Command(command, updateTime);
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.add(activity, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache[objectID + ";" + i] = new TimeValue(updateTime, attributtes[i]);
                    }
                }
            }

            if (objectType == "CP")
            {
                Models.CP cp = new Models.CP();
                string[] attributtes = serializedObject.Split(',');
                cp.buildFromStrings(attributtes);
                string objectID = objectType + ";" + cp.getID();

                if (!objectsCache.ContainsKey(objectID))
                {
                    objectsCache[objectID] = new Command(command, updateTime);
                    DatabaseHolder<Models.CP>.Instance.rememberedList.add(cp, false);
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache[objectID + ";" + i] = new TimeValue(updateTime, attributtes[i]);
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

            if (objectsCache.ContainsKey(objectCacheID))
            {
                objectsCache[objectCacheID] = new Command(command, updateTime);
                THIS.SendCommand(new Command(command, updateTime));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.Remove(objectID + ";" + i);
                }
            }
        }

        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            lastUpdate = updateTime;
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.ContainsKey(objectCacheID))
            {
                int nAttributes = 0;
                Models.LarpActivity removedActivity;
                if (objectType == "LarpActivity" && (removedActivity = DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.getByID(objectID)) != null) {
                    nAttributes = removedActivity.numOfAttributes();
                    DatabaseHolder<Models.LarpActivity>.Instance.rememberedList.removeAt(objectID);
                }
                Models.CP removedCP;
                if (objectType == "CP" && (removedCP = DatabaseHolder<Models.CP>.Instance.rememberedList.getByID(objectID)) != null)
                {
                    nAttributes = removedCP.numOfAttributes();
                    DatabaseHolder<Models.CP>.Instance.rememberedList.removeAt(objectID);
                }
                for (int i = 0; i < nAttributes; i++)
                {
                    attributesCache.Remove(objectID + ";" + i);
                }
                objectsCache.Remove(objectCacheID);
            }
        }
    }
}
