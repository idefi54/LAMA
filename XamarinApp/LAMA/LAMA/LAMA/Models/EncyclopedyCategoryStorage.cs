using LAMA.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    class EncyclopedyCategoryStorage: StorageInterface
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChildCategories { get; set; }
        public string Records { get; set; }

        public long lastChange { get; set; }

        public void makeFromStrings(string[] input)
        {
            ID = Convert.ToInt64(input[0]);
            Name = input[1];
            Description = input[2];
            ChildCategories = input[3];
            Records = input[4];
            
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), Name, Description, ChildCategories, Records };
        }

        public long getID()
        {
            return ID;
        }


    }
}
