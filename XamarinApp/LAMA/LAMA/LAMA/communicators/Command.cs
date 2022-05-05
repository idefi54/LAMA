using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    public class Command
    {
        public string command;
        public long time;

        public Command()
        {
            command = "";
            time = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public Command(string text)
        {
            string[] messageParts = text.Split(';');
            command = text;
            time = Int64.Parse(messageParts[0]);
        }

        public Command(string text, long initTime)
        {
            command = text;
            time = initTime;
        }

        public byte[] Encode()
        {
            return Encoding.Default.GetBytes(time + ";" + command);
        }

        public override string ToString()
        {
            return time.ToString() + ";" + command;
        }
    }
}
