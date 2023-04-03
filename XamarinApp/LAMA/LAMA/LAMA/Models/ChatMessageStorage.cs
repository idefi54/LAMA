using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Models
{
    internal class ChatMessageStorage: Database.StorageInterface
    {

        [PrimaryKey]
        public long ID { get; set; }
        public string from { get; set; }
        public int channel { get; set; }
        public string message { get; set; }
        
        public long sentAt { get; set; }

        public bool receivedByServer { get; set; }

        public long lastChange
        {
            get { return sentAt; }
            set { }
        }
        public void makeFromStrings(string[] input)
        {

            ID = Helpers.readLong(input[0]);
            from = input[1];
            channel = Helpers.readInt(input[2]);
            message = input[3];
            sentAt = Helpers.readLong(input[4]);
            receivedByServer = input[5] == "True" ? true : false;
        }
        public string[] getStrings()
        {
            return new string[] { ID.ToString(), from, channel.ToString(), message, sentAt.ToString(), receivedByServer.ToString() };
        }
        public long getID()
        {
            return sentAt;
        }
    }
}
