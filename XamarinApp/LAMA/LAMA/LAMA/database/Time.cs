using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    public class Time
    {

        public delegate void DataChanged();
        public event DataChanged dataChanged;


        int _minutes; 
        public int getRawMinutes()
        {
            return _minutes;
        }
        public void setRawMinutes(int input)
        {
            _minutes = input;
            dataChanged?.Invoke();
        }

        public int hours { get { return _minutes / 60; } set { _minutes = value * 60 + _minutes%60; dataChanged?.Invoke(); } }
        public int minutes { get { return _minutes % 60; }  set { _minutes = value + _minutes / 60; dataChanged?.Invoke(); }}

        public override string ToString()
        {
            return _minutes.ToString();
        }
    }
}
