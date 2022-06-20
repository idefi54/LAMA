using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    public class Command : SerializableDictionaryItem
    {
        private string _key;
        public string key { get { return _key; } }

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
            _time = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public Command(string text, string key)
        {
            string[] messageParts = text.Split(';');
            _command = text;
            _time = Int64.Parse(messageParts[0]);
            _key = key;
        }

        public Command(string text, long initTime, string key)
        {
            _command = text;
            _time = initTime;
            _key = key;
        }

        static string[] attributes = new string[] { "key", "time", "command" };

        void updateValue(int index, string newVal)
        {
            var list = DatabaseHolderStringDictionary<Command>.Instance.rememberedDictionary;
            list.sqlConnection.changeData(list.tableName, index, newVal, this);
        }

        public byte[] Encode()
        {
            return Encoding.Default.GetBytes(_time + ";" + _command);
        }

        public void buildFromStrings(string[] input)
        {
            _key = input[0];
            _time = Int64.Parse(input[1]);
            _command = input[2];
        }

        public string getAttribute(int index)
        {
            switch (index)
            {
                case 0: return _key.ToString();
                case 1: return _time.ToString();
                case 2: return _command;
            }
            throw new Exception("wrong index called");
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
                case 0: throw new Exception("Can not change key of Command");
                case 1: _time = Int64.Parse(value); break;
                case 2: _command = value; break;
            }
            throw new Exception("wrong index called");
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
