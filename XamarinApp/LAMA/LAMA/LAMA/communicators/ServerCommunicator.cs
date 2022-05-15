using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Xamarin.Essentials;

namespace LAMA
{
    public class ServerCommunicator
    {
        static byte[] buffer = new byte[8 * 1024];
        static Socket serverSocket;
        public Thread server;
        public static ServerCommunicator THIS;

        public Dictionary<string, Command> objectsCache = new Dictionary<string, Command>();
        public Dictionary<string, TimeValue> attributesCache = new Dictionary<string, TimeValue>();

        private static List<Socket> clientSockets = new List<Socket>();

        public object commandsLock = new object();
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
                    client.Send(data);
                }
            }
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);
            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), socket);
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
            catch (SocketException)
            {
                current.Close();
                clientSockets.Remove(current);
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
            current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
        }

        public ServerCommunicator(string name, string IP, int port, string password)
        {
            HttpClient client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "name", "\"" + name + "\"" },
                { "IP", "\"" + IP + "\"" },
                { "port", port.ToString() },
                { "password", "\"" + password + "\"" }
            };

            var content = new FormUrlEncodedContent(values);
            var response = client.PostAsync("https://larp-project-mff.000webhostapp.com/startserver.php", content);
            var responseString = response.Result.Content.ReadAsStringAsync();

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

        private void SendCommand(string commandText)
        {
            Command command = new Command(commandText, DateTimeOffset.Now.ToUnixTimeMilliseconds());
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
            attributesCache[attributeID].value = changed.getAttribute(attributeIndex);
            attributesCache[attributeID].time = updateTime;
            string command = "DataUpdated" + ";" + objectType + ";" + objectID + ";" + attributeID + ";" + changed.getAttribute(attributeIndex);
            THIS.SendCommand(new Command(command, updateTime));
        }

        public void DataUpdated(string objectType, int objectID, int indexAttribute, string value, long updateTime, string command)
        {
            string attributeID = objectType + ";" + objectID + ";" + indexAttribute;
            if (attributesCache.ContainsKey(attributeID) && attributesCache[attributeID].time <= updateTime)
            {
                attributesCache[attributeID].value = value;
                attributesCache[attributeID].time = updateTime;

                /*
                 * TO DO - update game data (once there is a way for me to access it)
                 */
            }
            // Notify every CP
            THIS.SendCommand(new Command(command, updateTime));
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
                // Notify every CP of item creation
                THIS.SendCommand(new Command(command, updateTime));
            }
        }

        public void ItemCreated(string objectType, string serializedObject, long updateTime, string command)
        {
            if (objectType == "LarpActivity")
            {
                LarpActivity activity = new LarpActivity();
                string[] attributtes = serializedObject.Split(',');
                activity.buildFromStrings(attributtes);
                string objectID = objectType + ";" + activity.getID();

                if (!objectsCache.ContainsKey(objectID))
                {
                    objectsCache[objectID] = new Command(command, updateTime);
                    /*
                     * TO DO - update game data once there is access to it
                     */
                    for (int i = 0; i < attributtes.Length; i++)
                    {
                        attributesCache[objectID + ";" + i] = new TimeValue(updateTime, attributtes[i]);
                    }
                    // Notify every CP of item creation
                    THIS.SendCommand(new Command(command, updateTime));
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

            if (objectsCache.ContainsKey(objectCacheID))
            {
                objectsCache[objectCacheID] = new Command(command, updateTime);
                // Notify every CP of item deletion
                THIS.SendCommand(new Command(command, updateTime));
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributesCache.Remove(objectID + ";" + i);
                }
            }
        }

        public void ItemDeleted(string objectType, int objectID, long updateTime, string command)
        {
            string objectCacheID = objectType + ";" + objectID;
            if (objectsCache.ContainsKey(objectCacheID))
            {
                /*
                 * TO DO - Find object and its attributes
                 * Delete attributes from cache
                 * Delete object
                 */
                objectsCache.Remove(objectCacheID);

                // Notify every CP of deletion
                THIS.SendCommand(new Command(command, updateTime));
            }
        }

        public void SendUpdate(Socket current, long lastUpdateTime)
        {
            foreach (KeyValuePair<string, Command> entry in objectsCache)
            {
                if (entry.Value.time > lastUpdateTime)
                {
                    current.Send(entry.Value.Encode());
                }
            }

            foreach (KeyValuePair<string, TimeValue> entry in attributesCache)
            {
                if (entry.Value.time > lastUpdateTime)
                {
                    string value = entry.Value.value;
                    string[] keyParts = entry.Key.Split(';');
                    string command = "DataUpdated" + ";" + keyParts[0] + ";" + keyParts[1] + ";" + keyParts[2] + ";" + value;
                    current.Send((new Command(command, entry.Value.time)).Encode());
                }
            }
        }
    }
}
