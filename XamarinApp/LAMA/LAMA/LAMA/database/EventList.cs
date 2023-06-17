using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    /// <summary>
    /// List with attatched events on Add, Remove and Update functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        public void InvokeDataChanged()
        {
            dataChanged?.Invoke();
        }

        public new void RemoveAll(Predicate<T> predicate)
        {
            int lengthStart = Count;
            base.RemoveAll(predicate);
            if (lengthStart > Count) dataChanged?.Invoke();
        }

        public new void RemoveAt(int where)
        {
            base.RemoveAt(where);
            dataChanged?.Invoke();
        }
        public new void Remove(T what)
        {
            for (int i = 0; i < Count; ++i) 
            {
                if (what.Equals(this[i])) 
                {
                    RemoveAt(i);
                    dataChanged?.Invoke();
                    return;
                }
            }
        }

        public void AddWithoutEvent(T what)
        {
            base.Add(what);
        }
        public string ToReadableString()
        {

            if (Count == 0)
                return string.Empty;
            else
            {
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < Count; ++i)
                {
                    if (i != 0)
                        output.Append(", ");

                    output.Append(this[i].ToString());

                }
                return output.ToString();
            }
        }
        public override string ToString()
        {
            if(Count == 0)
                return string.Empty;
            else
            {
                StringBuilder output = new StringBuilder();
                bool first = true;
                for (int i = 0; i < Count; ++i) 
                {
                    if(!first)
                        output.Append(Helpers.separator);

                    if(first)
                        first = false;
                    output.Append(this[i].ToString());

                }
                return output.ToString();
            }
        }

        public EventList()
        {
            
        }

        public EventList(List<T> list)
        {
            foreach(T item in list)
            {
                this.Add(item);
            }
        }

        public new void Clear()
        {
            base.Clear();
            dataChanged?.Invoke();
        }

        public void Update(List<T> otherOne)
        {
            
            for (int i = 0; i < otherOne.Count; ++i)
            {
                if (i < Count)
                {
                    if (this[i].Equals(otherOne[i]))
                        continue;
                    else
                        base[i] = otherOne[i];
                }
                else
                    base.Add(otherOne[i]);
            }
			for (int i = Count - 1; i >= otherOne.Count; i--)
			{
                base.RemoveAt(i);
			}

            dataChanged?.Invoke();
        }
    }
}
