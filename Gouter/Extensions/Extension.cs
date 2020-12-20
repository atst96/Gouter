using System.Collections.Generic;

namespace Gouter.Extensions
{
    /// <summary>
    /// 拡張メソッド全般
    /// </summary>
    internal static class Extension
    {
        /// <summary>
        /// IEnumerable<T>から要素のインデックスを取得する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">コレクション</param>
        /// <param name="value">探索する要素</param>
        /// <returns>要素番号</returns>
        public static int IndexOf<T>(this IEnumerable<T> collection, T value)
        {
            return IndexOf(collection, value, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// IEnumerable<T>から要素のインデックスを取得する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">コレクション</param>
        /// <param name="value">探索する要素</param>
        /// <param name="comparer"></param>
        /// <returns>要素番号</returns>
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

        /// <summary>
        /// LinkedList<T>から指定ノード以降のノードを削除する。
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="linkedList">LinkedList<T></param>
        /// <param name="currentNode">現在のノード</param>
        public static void RemoveAfterAll<T>(this LinkedList<T> linkedList, LinkedListNode<T> currentNode)
        {
            var node = currentNode;

            while (node != null)
            {
                linkedList.Remove(node);
                node = node.Next;
            }
        }
    }
}
