using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    public struct Time
    {
        public int day;
        int _minutes; 
        public int getRawMinutes()
        {
            return _minutes;
        }
        public void setRawMinutes(int input)
        {
            _minutes = input;
        }

        public int hours { get { return _minutes / 60; } set { _minutes = value * 60 + _minutes%60; } }
        public int minutes { get { return _minutes % 60; }  set { _minutes = value + _minutes / 60; }}
    }
}
