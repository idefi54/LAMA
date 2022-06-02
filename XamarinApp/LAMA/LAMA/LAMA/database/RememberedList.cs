using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{

    
    public class RememberedList<T> where T : Serializable, new()
    {
        
        Dictionary<int, int> IDToIndex = new Dictionary<int, int>();

        static StringBuilder ID = new StringBuilder();

        string myID;
        public string tableName { get { return myID; } }

        List<T> cache = new List<T>();
        OurSQL<T> sql;
        public OurSQL<T> sqlConnection { get { return sql; } }

        public RememberedList(OurSQL<T> sql)
        {
            this.sql = sql;

            if (ID.Length == 0)
                ID.Append(typeof(T).Name);
            if (ID[ID.Length - 1] < 'Z')
                ++ID[ID.Length - 1];
            else
                ID.Append('a');
            myID = ID.ToString();
            if (!sql.containsTable(myID))
                sql.makeTable(myID);

            loadData();
        }

        void loadData()
        {

            cache = sql.ReadData(myID);

            int i = 0;
            foreach(var a in cache)
            {
                IDToIndex.Add(a.getID(), i);
                ++i;
            }
        }

        public T this[int index]
        {

            get { return cache[index]; }
        }

        /*public void add(T data)
        {
            
            cache.Add(data);
            IDToIndex.Add(data.getID(), cache.Count - 1);

            sql.addData(myID, data, true);
        }*/

        /// <summary>
        /// adds the data to the database
        /// returns false if it fails (usually because of duplicate ID)
        /// </summary>
        /// <param name="data"> data to be added</param>
        /// <param name="invokeEvent"> should the network be notified about the change?</param>
        /// <returns></returns>
        public bool add(T data, bool invokeEvent = true)
        {
            if (IDToIndex.ContainsKey(data.getID()))
                return false;

            cache.Add(data);
            IDToIndex.Add(data.getID(), cache.Count - 1);

            sql.addData(myID, data, invokeEvent);
            return true;
        }

        public int Count { get { return cache.Count; } }

        public void removeAt(int index)
        {
            if (index < 0 || index >= cache.Count)
                return;
            
            IDToIndex.Remove(cache[index].getID());
            for (int i = index + 1; i < cache.Count; ++i) 
            {
                --IDToIndex[cache[i].getID()];
            }
            var who = cache[index];
            cache.RemoveAt(index);

            sql.removeAt(myID, who);
        }

        //There was no way for me to do this with the original interface (I couldn't get the item index based on ID)
        public void removeByID(int ID)
        {
            if (IDToIndex.ContainsKey(ID))
            {
                int internalIndex = IDToIndex[ID];
                removeAt(internalIndex);
            }
        }

        public T getByID(int id)
        {
            if (!IDToIndex.ContainsKey(id))
                return default(T);
            return cache[IDToIndex[id]];
        }


        List<Pair<int, int>> IDIntervals = new List<Pair<int, int>>();


        public void NewIntervalReceived(Pair<int, int> input)
        {
            throw new NotImplementedException();
        }



        public delegate void GiveNewIntervalDelegate<U>();
        public event GiveNewIntervalDelegate<T> GiveNewInterval;

        public int nextID()
        {

            /*if(málo id zbejvá = půlka posledního intervalu je plná)
             * {
             * invoke event (typ)
             * 
             * }
             * 
             * */


           /*
            if (došly ID)
           {
           počkat na server
           }
            */

            if(IDIntervals.Count == 0)
            {
                IDIntervals.Add(getNextIDInterval());
            }

            for (int i = 0; i < IDIntervals.Count; ++i) 
            {
                for (int j = IDIntervals[i].first; j < IDIntervals[i].second; ++j)
                {
                    if (!IDToIndex.ContainsKey(j))
                        return j;
                }
            }

            IDIntervals.Add(getNextIDInterval());
            return IDIntervals[IDIntervals.Count - 1].first;

        }

        // TODO   asynch await  protože bude trvat než dostanu odpověď
        // TODO   žádat o novej interval před tim než sem zaplněnej
        private Pair<int,int> getNextIDInterval()
        {
            throw new Exception("not implemented, call network to give me another interval");
        }

    }





}

