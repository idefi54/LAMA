using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.database
{
    internal class IntervalManager<T> where  T:Serializable
    {

        
        static Dictionary<int, List<Pair<int, int>>> intervalsGivenToClients;

        static List<Pair<int, int>> intervalsTaken;
        static int lastGiven;


        static public Pair<int, int> GiveNewInterval(int toWho)
        {
            throw new NotImplementedException();
        }

    }
}
