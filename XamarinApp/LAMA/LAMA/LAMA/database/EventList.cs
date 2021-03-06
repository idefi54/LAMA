using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    public class EventList<T> : List<T>
    {
        public delegate void DataChanged();
        public event DataChanged dataChanged;
        public new T this[int index]
        {
            get { return base[index]; }
            set 
            {
                base[index] = value;
                dataChanged?.Invoke();
            }
        }
        public new void Add(T what)
        {
            base.Add(what);
            dataChanged?.Invoke();
        }
        public new void RemoveAt(int where)
        {
            base.RemoveAt(where);
            dataChanged?.Invoke();
        }

        public void AddWithoutEvent(T what)
        {
            base.Add(what);
        }
    }
}
