using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    public class ModelPropertyChangeInfo : SerializableDictionaryItem
    {
        private string _key;
        public string key { get { return _key; } }

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

        private string _value;
        public string value
        {
            get { return _value; }
            set
            {
                _value = value;
                updateValue(2, value);
            }
        }

        /// <summary>
        /// Object describing a single attribute (its identification key, value and the time of the last change)
        /// </summary>
        /// <param name="init">Value of the attribute</param>
        /// <param name="key">Unique identifier</param>
        public ModelPropertyChangeInfo(string init, string key)
        {
            string[] initParts = init.Split(';');
            _time = Int64.Parse(initParts[0]);
            _value = initParts[1];
            _key = key;
        }

        /// <summary>
        /// Object describing a single attribute (its identification key, value and the time of the last change)
        /// </summary>
        /// <param name="initTime">Last time the attribute was changed in unix time format</param>
        /// <param name="initValue">Value of the attribute</param>
        /// <param name="key">Unique identifier</param>
        public ModelPropertyChangeInfo(long initTime, string initValue, string key)
        {
            _time = initTime;
            _value = initValue;
            _key = key;
        }

        public ModelPropertyChangeInfo() {}

        static string[] attributes = new string[] { "key", "time", "value" };

        void updateValue(int index, string newVal)
        {
            var list = DatabaseHolderStringDictionary<ModelPropertyChangeInfo, ModelPropertyChangeInfoStorage>.Instance.rememberedDictionary;
            list.sqlConnection.changeData(index, newVal, this);
        }

        public void buildFromStrings(string[] input)
        {
            _key = input[0];
            _time = Int64.Parse(input[1]);
            _value = input[2];
        }

        public string getAttribute(int index)
        {
            switch (index)
            {
                case 0: return _key.ToString();
                case 1: return _time.ToString();
                case 2: return _value;
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
                case 2: _value = value; break;
                default: throw new Exception("wrong index called");
            }
        }

        public override string ToString()
        {
            return _time.ToString() + ";" + _value;
        }

        public string getKey()
        {
            return _key;
        }
    }
}
