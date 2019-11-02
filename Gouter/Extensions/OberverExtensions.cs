using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gouter
{
    internal static class OberverExtensions
    {
        public static void DescribeAll<T>(this IList<T> observers, ISubscribable<T> subscribableObject)
            where T : ISubscribableObject
        {
            foreach (var observer in observers.ToArray())
            {
                subscribableObject.Describe(observer);
            }
        }

        public static void NotifyAll<T>(this IList<T> observers, Action<T> notifyAction)
            where T : ISubscribableObject
        {
            foreach (var observer in observers)
            {
                notifyAction(observer);
            }
        }
    }
}
