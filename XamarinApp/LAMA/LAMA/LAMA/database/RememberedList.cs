using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LAMA
{

    
    public class RememberedList<T, Storage> where T : Serializable, new() where Storage : Database.StorageInterface, new()
    {
        SortedDictionary<long, int> IDToIndex = new SortedDictionary<long, int>();

        
        List<T> cache = new List<T>();
        OurSQL<T, Storage> sql;
        OurSQLInterval intervalsSQL;
        public OurSQL<T, Storage> sqlConnection { get { return sql; } }
        int typeID = -1;

        public RememberedList(OurSQL<T, Storage> sql)
        {
            intervalsSQL = new OurSQLInterval();
            Debug.WriteLine("Interval constructor called");
            this.sql = sql;
            typeID = new T().getTypeID();
            Debug.WriteLine("Before load data");
            loadData();
        }

        void loadData()
        {
            
            IDIntervals = intervalsSQL.ReadData(typeID);

            cache = sql.ReadData();

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

            sql.addData(data, invokeEvent);
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

            sql.removeAt(who);
        }

        //There was no way for me to do this with the original interface (I couldn't get the item index based on ID)
        public void removeByID(long ID)
        {
            if (IDToIndex.ContainsKey(ID))
            {
                int internalIndex = IDToIndex[ID];
                removeAt(internalIndex);
            }
        }

        public T getByID(long id)
        {
            if (!IDToIndex.ContainsKey(id))
                return default(T);
            return cache[IDToIndex[id]];
        }


        List<Database.Interval> IDIntervals = new List<Database.Interval>();
        bool askedForMore = false;
        int currentIntervalNum = 0;

        public void NewIntervalReceived(Database.Interval input)
        {
            askedForMore = false;
            input.typeID = new T().getTypeID();
            IDIntervals.Add(input);
            intervalsSQL.addData(input);
        }



        public delegate void GiveNewIntervalDelegate();
        public event GiveNewIntervalDelegate GiveNewInterval;

        /// <summary>
        /// 
        /// </summary>
        /// <returns> returns -1 if it is waiting for more ID intervals from the server </returns>
        public long nextID()
        {   

            Database.Interval currentInterval = null;

            // find first interval with space 
            for (int i = currentIntervalNum; i < IDIntervals.Count; ++i) 
            {
                if (IDIntervals[i].lastTaken < IDIntervals[i].end - 1)
                {
                    currentInterval = IDIntervals[i];
                    currentIntervalNum = i;
                    break;
                }    
            }

            Database.Interval lastInterval = null;
            if (IDIntervals.Count != 0)
                lastInterval = IDIntervals[IDIntervals.Count - 1];
            // running out of IDs, so I need to ask the server for more
            if (currentInterval == null || 
                (lastInterval.lastTaken - lastInterval.start) / (lastInterval.end - lastInterval.start) > 0.5)
            {
                if (askedForMore)
                {
                    GiveNewInterval.Invoke();
                    askedForMore = true;
                }
                //if i am out of IDs just return null
                if (currentInterval == null || lastInterval.lastTaken + 1 == lastInterval.end) 
                    return -1;
            }

            //now i know i have anough space for at least one more ID

            currentInterval.lastTaken++;
            intervalsSQL.Update(currentInterval);
            return currentInterval.lastTaken;
          
        }

        
    }





}

