using LAMA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    internal class EncyclopedyRecordStorage: StorageInterface
    {
        [PrimaryKey]
        public long ID { get; set; }
        public string Name { get; set; }
        public string TLDR { get; set; }
        public string FullText { get; set; }

        public long lastChange { get; set; }

        public void makeFromStrings(string[] input)
        {
            ID = Helpers.readLong(input[0]);
            Name = input[1];
            TLDR = input[2];
            FullText = input[3];
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), Name, TLDR, FullText};
        }
        public long getID()
        {
            return ID;
        }
    }
}
