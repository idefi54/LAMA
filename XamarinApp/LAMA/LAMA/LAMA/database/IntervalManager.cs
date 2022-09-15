using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LAMA.Database
{
    internal class IntervalManager<T> where T : Serializable
    {

        static OurSQLInterval sql = new OurSQLInterval();
        //Hopefully this fix works (= new Dictionary<int, List<Interval>>())
        static Dictionary<int, List<Interval>> intervalsGivenToClients = new Dictionary<int, List<Interval>>();
        
        static List<Interval> intervalsTaken;
        static int endOfLast = 0;
        public static int IntervalLength { get; set; } = 100;

        static public Interval GiveNewInterval(int toWho)
        {
            Debug.WriteLine($"Giving new interval to someone {toWho}, {typeof(T)}");
            if (endOfLast == 0)
                initialize();

            Interval newInterval = new Interval(endOfLast, endOfLast + IntervalLength, toWho);
            endOfLast += IntervalLength;

            if (!intervalsGivenToClients.ContainsKey(toWho))
                intervalsGivenToClients.Add(toWho, new List<Interval>());

            Debug.WriteLine("Before IntervalsGivenToClients add");
            intervalsGivenToClients[toWho].Add(newInterval);
            Debug.WriteLine("Before IntervalsTaken add");
            intervalsTaken.Add(newInterval);
            Debug.WriteLine("Before sql.addData");
            sql.addData(newInterval);
            Debug.WriteLine("finished");
            return newInterval;
        }

        public static void initialize()
        {
            intervalsTaken = sql.ReadData();
            Debug.WriteLine(intervalsTaken.Count);

            for (int i = 0; i < intervalsTaken.Count; ++i) 
            {
                int toWho = intervalsTaken[i].ownerID;
                Debug.WriteLine($"to who {toWho}");
                if (!intervalsGivenToClients.ContainsKey(toWho))
                    intervalsGivenToClients.Add(toWho, new List<Interval>());
                Debug.WriteLine("Before add intervalsTaken");
                intervalsGivenToClients[toWho].Add(intervalsTaken[i]);
                Debug.WriteLine("initialize finished");

            }
        }

    }
}
