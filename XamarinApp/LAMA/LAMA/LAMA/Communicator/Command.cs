using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    public class Command : SerializableDictionaryItem
    {
        private string _key;
        public string key { get { return _key; } }

        private int _receiverID;
        public int receiverID { get { return _receiverID; } }

        private string _command;
        public string command
        {
            get { return _command; }
            set 
            { 
                _command = value;
                updateValue(0, value);
            }
        }
        private long _time;
        public long time
        {
            get { return _time; }
            set
            {
                _time = value;
                updateValue(1, value.ToString());
            }
        }

        public Command()
        {
            _command = "";
            _time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _receiverID = 0;
        }

        public Command(string text, string key)
        {
            string[] messageParts = text.Split(';');
            _command = text;
            _time = Int64.Parse(messageParts[0]);
            _key = key;
            _receiverID = 0;
        }

        public Command(string text, long initTime, string key)
        {
            _command = text;
            _time = initTime;
            _key = key;
            _receiverID = 0;
        }

        public Command(string text, string key, int id)
        {
            string[] messageParts = text.Split(';');
            _command = text;
            _time = Int64.Parse(messageParts[0]);
            _key = key;
            _receiverID = id;
        }

        public Command(string text, long initTime, string key, int id)
        {
            _command = text;
            _time = initTime;
            _key = key;
            _receiverID = id;
        }

        static string[] attributes = new string[] { "key", "time", "command", "receiverID" };

        void updateValue(int index, string newVal)
        {
            var list = DatabaseHolderStringDictionary<Command, CommandStorage>.Instance.rememberedDictionary;
            list.sqlConnection.changeData(index, newVal, this);
        }

        public byte[] Encode()
        {
            return Encoding.Default.GetBytes(_time + ";" + _command + "|");
        }

        public void buildFromStrings(string[] input)
        {
            _key = input[0];
            _time = Int64.Parse(input[1]);
            _command = input[2];
            _receiverID = Int32.Parse(input[3]);
        }

        public string getAttribute(int index)
        {
            switch (index)
            {
                case 0: return _key.ToString();
                case 1: return _time.ToString();
                case 2: return _command;
                case 3: return _receiverID.ToString();
                default: throw new Exception("wrong index called");
            }
        }

        public string[] getAttributeNames()
        {
            return attributes;
        }

        public string[] getAttributes()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < numOfAttributes(); ++i)
            {
                output.Add(getAttribute(i));
            }
            return output.ToArray();
        }

        public int numOfAttributes()
        {
            return attributes.Length;
        }

        public void setAttribute(int index, string value)
        {
            switch (index)
            {
                case 0: _key = value; break;
                case 1: _time = Int64.Parse(value); break;
                case 2: _command = value; break;
                case 3: _receiverID = Int32.Parse(value); break;
                default: throw new Exception("wrong index called");
            }
        }


        public string getKey()
        {
            return _key;
        }

        public override string ToString()
        {
            return time.ToString() + ";" + command;
        }
    }
}
