using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    /// <summary>
    /// A simple pair. Has hashcode and can be used in a dictionary.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    public struct Pair<A, B>
    {


        A _first;
        public A first
        {
            get { return _first; }
        }
        B _second;
        public B second
        {
            get { return _second; }
        }

        public Pair(A first, B second) : this()
        {
            this._first = first;
            this._second = second;
        }

        public override string ToString()
        {
            return _first.ToString() + Helpers.separator + _second.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj is Pair<A,B>)
            {
                Pair<A,B> other = (Pair<A,B>)obj;
                return other.first.Equals(first) && other.second.Equals(second);
            }
            return false;
            
        }

        public override int GetHashCode()
        {
            return first.GetHashCode() * second.GetHashCode();
        }
    }
}
