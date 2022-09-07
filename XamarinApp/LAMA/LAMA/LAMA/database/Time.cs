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
        public void setRawMinutes(int input, bool invoke)
        {
            _minutes = input;
            if (invoke)
                dataChanged?.Invoke();
        }

        public int hours { get { return _minutes / 60; } set { _minutes = value * 60 + _minutes % 60; dataChanged?.Invoke(); } }
        public int minutes { get { return _minutes % 60; } set { _minutes = value + _minutes / 60; dataChanged?.Invoke(); } }

        public override string ToString()
        {
            return _minutes.ToString();
        }

        public Time() { }
        public Time(int minutes)
        {
            _minutes = minutes;
        }

        public bool Equals(Time b)
        {
            return b.minutes == minutes && b.hours == hours;
        }

        public static bool operator ==(Time a, Time b) {
            return a.Equals(b);
        }

        public static bool operator !=(Time a, Time b)
        {
            return !(a.Equals(b));
        }

        public static bool operator <(Time a, Time b)
        {
            return a.hours < b.hours || (a.hours == b.hours && a.minutes < b.minutes);
        }

        public static bool operator >(Time a, Time b)
        {
            return a.hours > b.hours || (a.hours == b.hours && a.minutes > b.minutes);
        }

        public static bool operator <=(Time a, Time b)
        {
            return !(a.minutes > b.minutes);
        }

        public static bool operator >=(Time a, Time b)
        {
            return !(a._minutes < b._minutes);
        }
    }
}
