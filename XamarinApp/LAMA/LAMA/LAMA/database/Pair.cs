using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{

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
            return _first.ToString() + ", " + _second.ToString();
        }
    }
}
