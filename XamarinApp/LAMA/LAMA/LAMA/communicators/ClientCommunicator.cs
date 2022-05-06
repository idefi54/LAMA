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

namespace LAMA
{
    public class ClientCommunicator
    {
        static Socket s;
        public Thread listener;
        private static string CPName;
        private static string assignedEvent = "";

        static byte[] buffer = new byte[8 * 1024];
        public static ClientCommunicator THIS;
        public Dictionary<string, Command> objectsCache = new Dictionary<string, Command>();
        public Dictionary<string, TimeValue> attributesCache = new Dictionary<string, TimeValue>();
        static Random rd = new Random();

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
            s.Send(command.Encode());
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
            current.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), current);
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
            string q = System.DateTime.UtcNow.ToString() + ";";
            q += "Update;";
            q += CPName + ";";
            byte[] data = Encoding.Default.GetBytes(q);
            s.Send(data);
        }

        private void StartListening()
        {
            //SendConnectionInfo();
            RequestUpdate();
            s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveData), s);
        }

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
                s = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(IPAddress.Parse(array[0].Trim('"')), int.Parse(array[1]));
                CPName = clientName;
                listener = new Thread(StartListening);
                listener.Start();
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
            }
        }
    }
}
