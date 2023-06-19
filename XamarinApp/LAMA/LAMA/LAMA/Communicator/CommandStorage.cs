using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Communicator
{
    /// <summary>
    /// Stores the Command class in a database as an array of strings
    /// </summary>
    public class CommandStorage: Database.DictionaryStorageInterface
    {
        /// <summary>
        /// A unique string key used to identify the object
        /// </summary>
        [PrimaryKey]
        public string key { get; set; }
        /// <summary>
        /// Time the command was sent in the Unix time format
        /// </summary>
        public Int64 time { get; set; }
        /// <summary>
        /// The text of the command itself (the message sent over the network)
        /// </summary>
        public string command { get; set; }
        /// <summary>
        /// ID of the client the command is sent to. If it is sent to all of the clients -1.
        /// </summary>
        public Int32 receiverID { get; set; }


        /// <summary>
        /// Initialize the CommandStorage object with and array of strings - each of them with a value of one of the parameters
        /// </summary>
        public void makeFromStrings(string[] input)
        {
            key = input[0];
            time = Int64.Parse(input[1]);
            command = input[2];
            receiverID = Int32.Parse(input[3]);
        }

        /// <summary>
        /// Get a string array from the property values
        /// </summary>
        public string[] getStrings()
        {
            return new string[] { key, time.ToString(), command, receiverID.ToString() };
        }

        /// <summary>
        /// Get a unique string key identifying the object
        /// </summary>
        public string getKey()
        {
            return key;
        }

    }
}
