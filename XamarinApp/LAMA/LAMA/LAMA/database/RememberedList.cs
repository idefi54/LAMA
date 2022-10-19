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
        int maxID = 0;
        static long IDOffset = (long)Math.Pow(2, 31);

        SortedDictionary<long, int> IDToIndex = new SortedDictionary<long, int>();

        
        List<T> cache = new List<T>();
        OurSQL<T, Storage> sql;

        public OurSQL<T, Storage> sqlConnection { get { return sql; } }
        int typeID = -1;

        public RememberedList(OurSQL<T, Storage> sql)
        {

            Debug.WriteLine("Interval constructor called");
            this.sql = sql;
            typeID = new T().getTypeID();
            Debug.WriteLine("Before load data");
            loadData();
        }

        void loadData()
        {
            
            
            cache = sql.ReadData();

            int i = 0;
            foreach(var a in cache)
            {
                IDToIndex.Add(a.getID(), i);
                ++i;
                if(a.getID() / IDOffset == LocalStorage.clientID && maxID< a.getID())
                    maxID = (int)a.getID();
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
            if (data.getID() > maxID && data.getID() / IDOffset == LocalStorage.clientID)
                maxID = (int)data.getID();

            sql.addData(data, invokeEvent);
            data.addedInto(this);
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
            who.removed();
            sql.removeAt(who);
        }


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


        

        public long nextID()
        {
            ++maxID;
            return (long)(LocalStorage.clientID * IDOffset) + maxID;

            
        }

        
    }





}

