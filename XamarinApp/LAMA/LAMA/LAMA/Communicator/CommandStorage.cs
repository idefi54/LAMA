using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    internal class CommandStorage : Database.StorageInterface
    {
        public string key { get; set; }
        public int receiverID { get; set; }
        public string command { get; set; }
        public long time { get; set; }

        public long lastChange { get; set; }

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
        public int getID()
        {
            throw new NotImplementedException();
        }
    }
}
