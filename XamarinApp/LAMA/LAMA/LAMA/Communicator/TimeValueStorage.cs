using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Communicator
{
    internal class TimeValueStorage : Database.StorageInterface
    {
        public string key { get; set; }
        public string value { get; set; }
        public long time { get; set; }

        public long lastChange { get; set; }

        public void makeFromStrings(string[] input)
        {
            key = input[0];
            time = Int64.Parse(input[1]);
            value = input[2];
        }
        public string[] getStrings()
        {
            return new string[] { key, time.ToString(), value };
        }
        public int getID()
        {
            throw new NotImplementedException();
        }
    }
}
