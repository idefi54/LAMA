using System;
using System.Collections.Generic;
using System.Text;

namespace LAMA.Models
{
    public class CompositeComparer<T> : IComparer<T>
    {
        List<IComparer<T>> _comparers = new List<IComparer<T>>();

        /// <summary>
        /// Adds new comparer to the top of list of comparators. Can also remove existing comparers of same type.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="removeDuplicates">If true, it will first remove any existing comparers of the same type.</param>
        public void AddComparer(IComparer<T> comparer, bool removeDuplicates = true)
        {
            Type type = comparer.GetType();
            if (removeDuplicates)
                _comparers.RemoveAll(x => x.GetType() == type);
            _comparers.Insert(0, comparer);
        }

        public void ClearComparers()
        {
            _comparers.Clear();
        }

        public int Compare(T x, T y)
        {
            int result = 0;
            foreach (var item in _comparers)
            {
                result = item.Compare(x, y);
                if (result != 0)
                    break;
            }
            return result;
        }
    }
}
