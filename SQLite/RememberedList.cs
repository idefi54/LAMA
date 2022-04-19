using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    public class RememberedList<T> where T : Serializable, new()
    {
        static List<char> ID = new List<char>();

        string myID;

        List<T> cache = new List<T>();
        OurSQL<T> sql;
        public OurSQL<T> sqlConnection { get { return sql; } }
        RememberedList(OurSQL<T> sql)
        {
            this.sql = sql;

            if (ID.Count == 0)
                ID.Add('a');
            if (ID[ID.Count - 1] < 'Z')
                ++ID[ID.Count - 1];
            else
                ID.Add('a');
            myID = ID.ToString();

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
            set { sql.changeData(myID, value, index); }
        }

        public int Count { get { return cache.Count; } }

        public void removeAt(int index)
        {
            sql.removeAt(myID, index);
        }
    }





}

