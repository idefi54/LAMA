using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{

    
    public class RememberedList<T> where T : Serializable, new()
    {
        
        

        static StringBuilder ID = new StringBuilder();

        string myID;

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
        }

        public T this[int index]
        {
            get { return cache[index]; }
            set 
            { 
                sql.changeData(myID, value, index);

            }
        }

        public void add(T data)
        {
            sql.addData(myID, data);
            cache.Add(data);
        }

        public int Count { get { return cache.Count; } }

        public void removeAt(int index)
        {
            sql.removeAt(myID, index);

        }

        //TODO 
        public T getByID(int id)
        {

        }
    }





}

