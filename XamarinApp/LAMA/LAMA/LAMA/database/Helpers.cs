using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAMA
{
    class Helpers
    {
        public static long readLong (string input)
        {
            int offset = 0;
            long num = 0;
            while (offset < input.Length && char.IsDigit(input[offset]))
            {
                num *= 10;
                num += input[offset] - '0';
                ++offset;
            }
            return num;
        }
        public static int readInt(string input, ref int offset)
        {
            int num = 0;
            while (offset < input.Length && char.IsDigit(input[offset]))
            {
                num *= 10;
                num += input[offset] - '0';
                ++offset;
            }
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
            while (offset < input.Length && input[offset] != ',')
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

        public static double readDouble(string input, ref int offset)
        {

            int firstPart = readInt(input, ref offset);
            // skip the decimal . or ,
            ++offset;
            int secondPart = readInt(input, ref offset);

            return (double)firstPart + ((double)secondPart) / Math.Pow(10, Math.Ceiling(Math.Log10(secondPart)));
        }
        public static Pair<double, double> readDoublePair(string input)
        {
            int i = 0;
            double first = readDouble(input, ref i);
            skipNonDigits(input, ref i);
            return new Pair<double, double>(first, readDouble(input, ref i));
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


        public static EventList<string> readStringField(string input)
        {
            int i = 0;
            EventList<string> output = new EventList<string>();

            while (i < input.Length)
            {
                output.Add(readString(input, ref i));
                ++i;
            }
            return output;
        }

    }
}
