using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Models
{
    internal class CPStorage: Database.StorageInterface
    {
        [PrimaryKey]
        public long ID { get; set; } 

        public string name { get; set; }
        public string nick { get; set; }
        public string roles { get; set; }
        public string phone { get; set; }
        public string facebook { get; set; }
        public string discord { get; set; }
        public string location { get; set; }
        public string notes { get; set; }
        public string permissions { get; set; }
        public string password { get; set; }
        public long lastChange { get; set; }


        public void makeFromStrings(string[] input)
        {
            ID = Helpers.readLong(input[0]);
            name = input[1];
            nick = input[2];
            roles = input[3];
            phone = input[4];
            facebook = input[5];
            discord = input[6];
            location = input[7];
            notes = input[8];
            permissions = input[9];
            password = input[10];
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), name, nick, roles, phone, facebook, discord, location, notes, permissions, password};
        }
        public long getID()
        {
            return ID;
        }
    }
}
