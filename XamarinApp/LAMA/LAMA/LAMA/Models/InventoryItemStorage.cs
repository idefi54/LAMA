using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace LAMA.Models
{
    internal class InventoryItemStorage : database.StorageInterface
    {

        //{ "ID", "name", "description", "taken", "free", "takenBy" };
        [PrimaryKey]
        public int ID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string taken { get; set; }
        public string free { get; set; }
        public string takenBy { get; set; }

        public long lastChange { get; set; }
        public void makeFromStrings(string[] input)
        {
            ID = Helpers.readInt(input[0]);
            name = input[1];
            description = input[2];
            taken = input[3];
            free = input[4];
            takenBy = input[5];
        }
        public string[] getStrings ()
        {
            return new string[] { ID.ToString(), name, description, taken, free, takenBy};
        }
        public int getID()
        {
            return ID;
        }
    }
}
