using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteTest
{
    class Helpers
    {
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

        public static List<int> readIntField(string input)
        {
            List<int> output = new List<int>();
            int i = 0;
            skipNonDigits(input, ref i);
            while(i<input.Length)
            {
                output.Add(readInt(input, ref i));
                skipNonDigits(input, ref i);
            }
            return output;
        }
        static void skipNonDigits(string input, ref int offset)
        {
            while (!char.IsDigit(input[offset]))
                ++offset;
        }

        public static int findIndex<T> (T[] array, T value) 
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

            return (double)firstPart + ((double)secondPart) / Math.Ceiling(Math.Log10(secondPart));
        }
        public static (double, double) readDoublePair(string input)
        {
            int i = 0;
            double first = readDouble(input, ref i);
            ++i;
            return (first, readDouble(input, ref i));
        }
    }
}
