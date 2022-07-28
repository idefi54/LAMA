using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace LAMA.Models
{
    internal class ChatMessageStorage: Database.StorageInterface
    {

        public string from { get; set; }
        public int channel { get; set; }
        public string message { get; set; }
        [PrimaryKey]
        public long sentAt { get; set; }

        public long lastChange
        {
            get { return sentAt; }
            set { }
        }
        public void makeFromStrings(string[] input)
        {
            from = input[0];
            channel = Helpers.readInt(input[1]);
            message = input[2];
            sentAt = Helpers.readLong(input[3]);
        }
        public string[] getStrings()
        {
            return new string[] { from, channel.ToString(), message, sentAt.ToString() };
        }
        public long getID()
        {
            return sentAt;
        }
    }
}
