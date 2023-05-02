using LAMA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    class Helpers
    {

        public static char separator = '⸬';


        public static EventList<long> readLongField(string input)
        {


            EventList<long> output = new EventList<long>();
            if (input == null)
                return output;
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                output.Add(readLong(input, ref i));
                skipNonDigits(input, ref i);
            }
            return output;
        }
        public static long readLong(string input)
        {
            int i = 0;
            return readLong(input, ref i);
        }
        public static long readLong(string input, ref int offset)
        {
            bool negative = false;

            while (offset < input.Length && !char.IsDigit(input[offset]))
            {
                if (input[offset] == '-')
                    negative = true;
                ++offset;
            }

            long num = 0;
            while (offset < input.Length && char.IsDigit(input[offset]))
            {
                num *= 10;
                num += input[offset] - '0';
                ++offset;
            }
            if (negative)
                return -num;
            return num;
        }
        public static int readInt(string input, ref int offset)
        {


            bool negative = false;
            int i = offset;
            while(offset < input.Length && i>0 && Char.IsWhiteSpace(input[i]) && i< input.Length)
            {
                --i;
            }
            if (offset >= input.Length)
                return 0;
            negative = input[i] == '-';

            int num = 0;
            while (offset < input.Length && char.IsDigit(input[offset]))
            {
                num *= 10;
                num += input[offset] - '0';
                ++offset;
            }
            if (negative)
                return -num;
            return num;
        }
        public static int readInt(string input)
        {
            int i = 0;
            return readInt(input, ref i);
        }

        public static EventList<int> readIntField(string input)
        {

            EventList<int> output = new EventList<int>();
            if (input == null)
                return output;
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                output.Add(readInt(input, ref i));
                skipNonDigits(input, ref i);
            }
            return output;
        }
        static void skipNonDigits(string input, ref int offset)
        {
            while (offset < input.Length && !char.IsDigit(input[offset]))
                ++offset;
        }
        static string readString(string input, ref int offset)
        {
            StringBuilder output = new StringBuilder();
            while (offset < input.Length && input[offset] != separator)
            {
                output.Append(input[offset]);
                ++offset;
            }
            return output.ToString();
        }

        public static int findIndex<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i].Equals(value))
                    return i;
            }
            return -1;
        }

        public static double readDouble(string input)
        {
            int i = 0; 
            return readDouble(input, ref i);
        }
        public static double readDouble(string input, ref int offset)
        {
            if (input == null)
                return 0;
            
            long firstPart = readLong(input, ref offset);
            // skip the decimal . or ,
            skipWhiteSpace(input, ref offset);
            if ( offset < input.Length && (input[offset] == '.' || input[offset] == ','))
            {
                ++offset;
                int offset1 = offset;
                long secondPart = readLong(input, ref offset);
                offset1 = offset - offset1;
                return (double)firstPart + ((double)secondPart) / Math.Pow(10, offset1);
            }
            else
                return firstPart;
        }
        public static Pair<double, double> readDoublePair(string input)
        {
            int i = 0;
            double first = readDouble(input, ref i);
            skipNonDigits(input, ref i);
            Pair<double, double> result = new Pair<double, double>(first, readDouble(input, ref i));
            return result;
        }

        public static EventList<Pair<int, int>> readIntPairField(string input)
        {
            EventList<Pair<int, int>> output = new EventList<Pair<int, int>>();
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                int first = readInt(input, ref i);
                skipNonDigits(input, ref i);
                output.Add(new Pair<int, int>(first, readInt(input, ref i)));
                skipNonDigits(input, ref i);
            }
            return output;
        }
        public static Pair<int,int> readIntPair(string input)
        {
            if (input == null)
                return new Pair<int, int>(0, 0);
            int i = 0;
            skipNonDigits(input, ref i);
            int first = readInt(input, ref i);
            skipNonDigits(input, ref i);
            int second = readInt(input, ref i);
            return new Pair<int, int>(first, second);
        }

        public static EventList<Pair<long, long>> readLongPairField(string input)
        {
            EventList<Pair<long, long>> output = new EventList<Pair<long, long>>();
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                long first = readLong(input, ref i);
                skipNonDigits(input, ref i);
                output.Add(new Pair<long, long>(first, readLong(input, ref i)));
                skipNonDigits(input, ref i);
            }
            return output;
        }

        public static EventList<Pair<long, int>> readLongIntPairField(string input)
        {
            EventList<Pair<long, int>> output = new EventList<Pair<long, int>>();
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                long first = readLong(input, ref i);
                skipNonDigits(input, ref i);
                output.Add(new Pair<long, int>(first, readInt(input, ref i)));
                skipNonDigits(input, ref i);
            }
            return output;
        }

        public static EventList<Pair<int, long>> readIntLongPairField(string input)
        {
            EventList<Pair<int, long>> output = new EventList<Pair<int, long>>();
            int i = 0;
            skipNonDigits(input, ref i);
            while (i < input.Length)
            {
                int first = readInt(input, ref i);
                skipNonDigits(input, ref i);
                output.Add(new Pair<int, long>(first, readLong(input, ref i)));
                skipNonDigits(input, ref i);
            }
            return output;
        }

        public static EventList<Pair<string, int>> readStringIntPairField(string input)
        {
            EventList<Pair<string, int>> output = new EventList<Pair<string, int>>();
            int i = 0;
            while (i < input.Length)
            {
                string first = readString(input, ref i);
                skipNonDigits(input, ref i);
                output.Add(new Pair<string, int>(first, readInt(input, ref i)));
                ++i;
            }
            return output;
        }
        public static EventList<Pair<int, string>> readIntStringPairField(string input)
        {
            EventList<Pair<int, string>> output = new EventList<Pair<int, string>>();
            int i = 0;
            while (i < input.Length)
            {
                int first = readInt(input, ref i);
                ++i;
                output.Add(new Pair<int, string>(first, readString(input, ref i)));
                skipNonDigits(input, ref i); ;
            }
            return output;
        }
        public static EventList<Pair<long, string>> readLongStringPairField(string input)
        {
            EventList<Pair<long, string>> output = new EventList<Pair<long, string>>();
            int i = 0;
            while (i < input.Length)
            {
                long first = readLong(input, ref i);
                ++i;
                output.Add(new Pair<long, string>(first, readString(input, ref i)));
                skipNonDigits(input, ref i); ;
            }
            return output;
        }


        public static EventList<string> readStringField(string input)
        {
            int i = 0;
            EventList<string> output = new EventList<string>();
            if (input == null)
                return output;
            while (i < input.Length)
            {
                output.Add(readString(input, ref i));
                ++i;
            }
            return output;
        }


        public static string EnumEventListToString<T>(EventList<T> input) where T:Enum 
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < input.Count; ++i)
            {
                if (i != 0)
                    output.Append(separator);
                output.Append((int)(object)input[i]);
            }
            return output.ToString();
        }

        public static EventList<double> readDoubleField(string input)
        {
            EventList<double> output = new EventList<double>();
            if (input == null)
                return output;
            int i = 0;
            while (i < input.Length)
            {
                skipNonDigits(input, ref i);
                output.Add(readDouble(input, ref i));
                ++i;
            }
            return output;
        }
        public static EventList<Pair<double, double>> readDoublePairField(string input)
        {
            EventList<Pair<double, double>> output = new EventList<Pair<double, double>>();
            if (input == null)
                return output;
            int i = 0;
            while (i < input.Length)
            {
                skipNonDigits(input, ref i);
                double first = readDouble(input, ref i);
                skipNonDigits(input, ref i);
                double second = readDouble(input, ref i);
                output.Add(new Pair<double, double>(first, second));
                ++i;
            }
            return output;
        }

        static void skipWhiteSpace(string input, ref int offset)
        {
            while (offset < input.Length && char.IsWhiteSpace(input[offset]))
            {
                ++offset;
            }
        }
    }
}
