using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    /// <summary>
    /// Class representing a single message sent over the network
    /// </summary>
    public class Command : SerializableDictionaryItem
    {
        private string _key;
        /// <summary>
        /// Unique string key identifying the object
        /// </summary>
        public string key { get { return _key; } }

        private int _receiverID;
        /// <summary>
        /// ID of the client the command is sent to. This is equal to -1 if the command is sent to everyone.
        /// </summary>
        public int receiverID { get { return _receiverID; } }

        private string _command;
        /// <summary>
        /// Text of the command
        /// </summary>
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
        /// <summary>
        /// The time the command was sent in the Unix format
        /// </summary>
        public long time
        {
            get { return _time; }
            set
            {
                _time = value;
                updateValue(1, value.ToString());
            }
        }

        /// <summary>
        /// Initialize an empty command
        /// </summary>
        public Command()
        {
            _command = "";
            _time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _receiverID = 0;
        }

        /// <summary>
        /// Initialize command with a specific text and key.
        /// </summary>
        /// <param name="text">Text of the command including the time part of the message</param>
        /// <param name="key">Key for the database</param>
        public Command(string text, string key)
        {
            string[] messageParts = text.Split(SpecialCharacters.messagePartSeparator);
            _command = text;
            _time = Int64.Parse(messageParts[0]);
            _key = key;
            _receiverID = 0;
        }

        /// <summary>
        /// Initialize the command with a text, time sent and key
        /// </summary>
        /// <param name="text">Text of the message</param>
        /// <param name="initTime">The time the message was sent</param>
        /// <param name="key">Key for the database</param>
        public Command(string text, long initTime, string key)
        {
            _command = text;
            _time = initTime;
            _key = key;
            _receiverID = 0;
        }

        /// <summary>
        /// Initialize command with text, unique key and ID
        /// </summary>
        /// <param name="text">Text of the message</param>
        /// <param name="key">Unique key for the database</param>
        /// <param name="id">Id of the client receiving the message</param>
        public Command(string text, string key, int id)
        {
            string[] messageParts = text.Split(SpecialCharacters.messagePartSeparator);
            _command = text;
            _time = Int64.Parse(messageParts[0]);
            _key = key;
            _receiverID = id;
        }

        /// <summary>
        /// Initialize all of the command values
        /// </summary>
        /// <param name="text">Text of the message</param>
        /// <param name="initTime">Time the message was sent</param>
        /// <param name="key">Unique key for the database</param>
        /// <param name="id">Id of the client receiving the message</param>
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

        /// <summary>
        /// Get an encoded representation of the command (using AES algorithm for encryption and Huffman coding for compression)
        /// </summary>
        /// <returns></returns>
        public byte[] Encode(Compression compressor)
        {
            return Encryption.HuffmanCompressAESEncode(_time + SpecialCharacters.messagePartSeparator.ToString() + _command + SpecialCharacters.messageSeparator.ToString(), compressor);
        }

        /// <summary>
        /// Initialize the property values with those specified in a string array
        /// </summary>
        public void buildFromStrings(string[] input)
        {
            _key = input[0];
            _time = Int64.Parse(input[1]);
            _command = input[2];
            _receiverID = Int32.Parse(input[3]);
        }

        /// <summary>
        /// Get a string of a value of a property.
        /// </summary>
        /// <param name="index">The index of the property</param>
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

        /// <summary>
        /// Gets a string array with the names of all of the object properties.
        /// </summary>
        public string[] getAttributeNames()
        {
            return attributes;
        }

        /// <summary>
        /// Returns a string array with the values of all of the command properties
        /// </summary>
        public string[] getAttributes()
        {
            List<string> output = new List<string>();
            for (int i = 0; i < numOfAttributes(); ++i)
            {
                output.Add(getAttribute(i));
            }
            return output.ToArray();
        }

        /// <summary>
        /// Returns the number of the object properties
        /// </summary>
        public int numOfAttributes()
        {
            return attributes.Length;
        }

        /// <summary>
        /// Sets a value of the object property
        /// </summary>
        /// <param name="index">Index of the property</param>
        /// <param name="value">String value of the property (this method will convert it to the appropriate data type)</param>
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

        /// <summary>
        /// Get a unique string identifying the object
        /// </summary>
        public string getKey()
        {
            return _key;
        }

        /// <summary>
        /// Get string representation of the command, not encoded
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return time.ToString() + SpecialCharacters.messagePartSeparator + command;
        }
    }
}
