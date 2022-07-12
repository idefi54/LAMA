using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Communicator
{
    internal class CommandStorage: Database.DictionaryStorageInterface
    {
        
        [PrimaryKey]
        public string key { get; set; }
        public Int64 time { get; set; }
        public string command { get; set; }
        public Int32 receiverID { get; set; }



        public void makeFromStrings(string[] input)
        {
            key = input[0];
            time = Int64.Parse(input[1]);
            command = input[2];
            receiverID = Int32.Parse(input[3]);
        }
        public string[] getStrings()
        {
            return new string[] { key, time.ToString(), command, receiverID.ToString() };
        }
        public string getKey()
        {
            return key;
        }

    }
}
