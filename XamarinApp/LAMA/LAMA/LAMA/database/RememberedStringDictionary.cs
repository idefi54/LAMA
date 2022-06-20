using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA
{
    class RememberedStringDictionary<T, Storage> where T : SerializableDictionaryItem, new() where Storage:database.StorageInterface, new()
    {        
        Dictionary<string, int> KeyToIndex = new Dictionary<string, int>();

        List<T> cache = new List<T>();
        OurSQLDictionary<T, Storage> sql;
        public OurSQLDictionary<T, Storage> sqlConnection { get { return sql; } }

        public RememberedStringDictionary(OurSQLDictionary<T, Storage> sql)
        {
            this.sql = sql;

            loadData();
        }

        void loadData()
        {
            cache = sql.ReadData();

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

            sql.addData(data);
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

            sql.removeAt(who);
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
