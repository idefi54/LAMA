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

        public void add(T data)
        {
            
            cache.Add(data);
            IDToIndex.Add(data.getID(), cache.Count - 1);

            sql.addData(myID, data);
        }

        public int Count { get { return cache.Count; } }

        public void removeAt(int index)
        {
            
            IDToIndex.Remove(cache[index].getID());
            for (int i = index; i < cache.Count; ++i)
            {
                --IDToIndex[cache[index].getID()];
            }
            cache.RemoveAt(index);

            sql.removeAt(myID, cache[index]);
        }

        public T getByID(int id)
        {
            return cache[IDToIndex[id]];
        }
    }





}

