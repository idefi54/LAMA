﻿using System;
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
        public new void Remove(T what)
        {
            for (int i = 0; i < Count; ++i) 
            {
                if (what.Equals(this[i])) 
                {
                    RemoveAt(i);
                    dataChanged.Invoke();
                    return;
                }
            }
        }

        public void AddWithoutEvent(T what)
        {
            base.Add(what);
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
            dataChanged.Invoke();
        }

        public void Update (EventList<T> otherOne)
        {
            
            for(int i = 0; i< otherOne.Count; ++i)
            {
                if (Count < i)
                {
                    if (this[i].Equals(otherOne[i]))
                        continue;
                    else
                        base[i] = otherOne[i];
                }
                else
                    base.Add(otherOne[i]);
            }
            dataChanged.Invoke();
        }
    }
}
