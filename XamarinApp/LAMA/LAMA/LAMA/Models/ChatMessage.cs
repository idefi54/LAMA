using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    //this should be a read only class since it only remembers what was sent and when
    public class ChatMessage: Serializable
    {

        long _ID;
        public long ID { get { return _ID; }}
        string _from;
        public string from { get { return _from; } }
        int _channel;
        public int channel { get { return _channel; } }
        string _message;
        public string message { get { return _message; } }

        long _sentAt;
        /// <summary>
        /// unix timestamp in milliseconds
        /// </summary>
        public long sentAt { 
            get { return _sentAt; }
            set { 
                _sentAt = value;
                updateValue(4, value.ToString());
            }
        }

        bool _receivedByServer;
        public bool receivedByServer { 
            get { return  _receivedByServer; } 
            set { 
                _receivedByServer = value; 
                updateValue(5, value.ToString());
            }
        }

        public long getID()
        {
            return _ID;
        }
        static string[] attributes = new string[] { "ID","from", "channel", "message", "sentAt", "receivedByServer" };
        public int getTypeID()
        {
            return 4;
        }
        public string[] getAttributes()
        {
            return new string[] { ID.ToString(), from, channel.ToString(), message, sentAt.ToString(), receivedByServer.ToString() };
        }
        public string[] getAttributeNames()
        {
            return attributes;
        }
        public void setAttribute(int which, string value)
        {
            switch(which)
            {
                case 0: 
                    _ID = Helpers.readLong(value); 
                    break;
                case 1:
                    _from = value;
                    break;
                case 2:
                    _channel = Helpers.readInt(value);
                    break;
                case 3:
                    _message = value;
                    break;
                case 4: 
                    _sentAt = Helpers.readLong(value);
                    break;
                case 5:
                    _receivedByServer = value == "True" ? true : false;
                    break;

            }
        }

        void updateValue(int index, string newVal)
        {
            list?.sqlConnection.changeData(index, newVal, this);
        }

        public int numOfAttributes()
        { return attributes.Length; }

        public string getAttribute(int which)
        {
            return getAttributes()[which];
        }
        public void buildFromStrings(string[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                setAttribute(i, input[i]);
            }
        }

        RememberedList<ChatMessage, ChatMessageStorage> list = null;
        public void addedInto(object holder)
        {
            list = (RememberedList<ChatMessage, ChatMessageStorage>)holder;
        }
        public void removed()
        {
            list = null;
        }


        

        public ChatMessage()
        { }
        public ChatMessage(string from, int channel, string message, long sentAt, bool receivedByServer)
        {
            _from = from;
            _channel = channel;
            _message = message;
            _sentAt = sentAt;
            _ID = _sentAt + LocalStorage.cpID * 1000000000000;
            _receivedByServer = receivedByServer;
        }

        public event EventHandler<int> IGotUpdated;
        public void InvokeIGotUpdated(int index)
        {
            IGotUpdated?.Invoke(this, index);
        }
    }
}
