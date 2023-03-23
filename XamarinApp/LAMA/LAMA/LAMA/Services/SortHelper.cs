using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Services
{
    public static class SortHelper
    {
        public static void BubbleSort<T>(IList<T> list,bool ascending, IComparer<T> comparer)
        {
            // just bubble sort because i wanna do it super simply and in place
            // and i am too lazy to do merge sort in place
            bool changed = true;
            while (changed)
            {
                changed = false;
                // one pass
                for (int i = 0; i < list.Count - 1; ++i)
                {
                    if ((ascending && comparer.Compare(list[i], list[i + 1]) < 0) ||
                        (!ascending && comparer.Compare(list[i], list[i + 1]) > 0))
                    {
                        //swap 
                        var temp = list[i];
                        list[i] = list[i + 1];
                        list[i + 1] = temp;
                        if (!changed)
                            changed = true;
                    }
                }
            }
        }

    }
}
