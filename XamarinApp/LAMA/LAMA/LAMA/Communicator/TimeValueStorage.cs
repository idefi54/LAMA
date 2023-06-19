using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace LAMA.Communicator
{
    /// <summary>
    /// Class which helps to store the ModelPropertyChangeInfo into a database.
    /// </summary>
    public class ModelPropertyChangeInfoStorage: Database.DictionaryStorageInterface
    {
        [PrimaryKey]
        public string key { get; set; }
        public Int64 time { get; set; }
        public string value { get; set; }
        

        public string getKey()
        { return key; }

        public string[] getStrings()
        {
            return new string[] { key, time.ToString(), value };
        }
        public void makeFromStrings(string[] input)
        {
            key = input[0];
            time = Int64.Parse(input[1]);
            value = input[2];
        }
    }
}
