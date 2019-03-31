using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Extensions
{
    internal static class Extension
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T value)
        {
            return IndexOf(collection, value, EqualityComparer<T>.Default);
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, T value, IEqualityComparer<T> comparer)
        {
            int index = 0;

            foreach (var item in collection)
            {
                if (comparer.Equals(item, value))
                {
                    return index;
                }

                ++index;
            }

            return -1;
        }
    }
}
