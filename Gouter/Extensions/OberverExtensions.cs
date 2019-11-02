using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gouter
{
    internal static class OberverExtensions
    {
        /// <summary>監視対象からすべての購読を解除する</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observers"></param>
        /// <param name="subscribableObject"></param>
        public static void DescribeAll<T>(this IList<T> observers, ISubscribable<T> subscribableObject)
            where T : ISubscribableObject
        {
            foreach (var observer in observers.ToArray())
            {
                subscribableObject.Describe(observer);
            }
        }

        /// <summary>すべての監視オブジェクトに通知する</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observers"></param>
        /// <param name="notifyAction"></param>
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
