using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    public class TimeValue
    {
        public long time;
        public string value;

        public TimeValue(string init)
        {
            string[] initParts = init.Split(';');
            time = Int64.Parse(initParts[0]);
            value = initParts[1];
        }

        public TimeValue(long initTime, string initValue)
        {
            time = initTime;
            value = initValue;
        }

        public override string ToString()
        {
            return time.ToString() + ";" + value;
        }
    }
}
