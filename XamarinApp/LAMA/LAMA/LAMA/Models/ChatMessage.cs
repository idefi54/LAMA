using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    //this should be a read only class since it only remembers what was sent and when
    internal class ChatMessage: Serializable
    {
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
        public long sentAt { get { return _sentAt; } }

        //WARNING might have to change this to long instead of int
        public long getID()
        {
            return sentAt;
        }
        static string[] attributes = new string[] { "from", "channel", "message", "sentAt" };
        public int getTypeID()
        {
            return 4;
        }
        public string[] getAttributes()
        {
            return new string[] { from, channel.ToString(), message, sentAt.ToString() };
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
                    _from = value;
                    break;
                case 1:
                    _channel = Helpers.readInt(value);
                    break;
                case 2:
                    _message = value;
                    break;
                case 3: 
                    _sentAt = Helpers.readLong(value);
                    break;

            }
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
    }
}
