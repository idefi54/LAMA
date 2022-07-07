using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Database
{
    internal class IntervalManager<T> where T : Serializable
    {

        static OurSQLInterval sql = new OurSQLInterval();
        static Dictionary<int, List<Interval>> intervalsGivenToClients;
        
        static List<Interval> intervalsTaken;
        static int endOfLast = 0;
        public static int IntervalLength { get; set; } = 100;

        static public Interval GiveNewInterval(int toWho, int typeID)
        {
            if (endOfLast == 0)
                initialize();

            Interval newInterval = new Interval(endOfLast, endOfLast + IntervalLength,typeID, toWho);
            endOfLast += IntervalLength;

            if (intervalsGivenToClients.ContainsKey(toWho))
                intervalsGivenToClients.Add(toWho, new List<Interval>());
            intervalsGivenToClients[toWho].Add(newInterval);
            intervalsTaken.Add(newInterval);
            sql.addData(newInterval);
            return newInterval;
        }

        public static void initialize()
        {
            intervalsTaken = sql.ReadData();

            for (int i = 0; i < intervalsTaken.Count; ++i) 
            {
                int toWho = intervalsTaken[i].ownerID;
                if (intervalsGivenToClients.ContainsKey(toWho))
                    intervalsGivenToClients.Add(toWho, new List<Interval>());
                intervalsGivenToClients[toWho].Add(intervalsTaken[i]);
            }

        }

    }
}
