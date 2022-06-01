using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    class RememberedStringDictionary<T> where T : SerializableDictionaryItem, new()
    {        
        Dictionary<string, int> KeyToIndex = new Dictionary<string, int>();

        static StringBuilder ID = new StringBuilder();

        string myID;
        public string tableName { get { return myID; } }

        List<T> cache = new List<T>();
        OurSQLDictionary<T> sql;
        public OurSQLDictionary<T> sqlConnection { get { return sql; } }

        public RememberedStringDictionary(OurSQLDictionary<T> sql)
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
            foreach (var a in cache)
            {
                KeyToIndex.Add(a.getKey(), i);
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
            if (KeyToIndex.ContainsKey(data.getKey()))
                return false;

            cache.Add(data);
            KeyToIndex.Add(data.getKey(), cache.Count - 1);

            sql.addData(myID, data);
            return true;
        }

        public int Count { get { return cache.Count; } }

        public void removeAt(int index)
        {
            if (index < 0 || index >= cache.Count)
                return;

            KeyToIndex.Remove(cache[index].getKey());
            for (int i = index + 1; i < cache.Count; ++i)
            {
                --KeyToIndex[cache[i].getKey()];
            }
            var who = cache[index];
            cache.RemoveAt(index);

            sql.removeAt(myID, who);
        }

        public void removeByKey(string key)
        {
            if (KeyToIndex.ContainsKey(key))
            {
                int internalIndex = KeyToIndex[key];
                removeAt(internalIndex);
            }
        }

        public T getByKey(string key)
        {
            if (!KeyToIndex.ContainsKey(key))
                return default(T);
            return cache[KeyToIndex[key]];
        }

        public bool containsKey(string key)
        {
            return KeyToIndex.ContainsKey(key);
        }
    }
}
