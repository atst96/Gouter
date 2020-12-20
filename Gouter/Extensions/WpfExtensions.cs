using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Gouter.Extensions
{
    /// <summary>
    /// WPFに関する拡張メソッド
    /// </summary>
    internal static class WpfExtensions
    {
        /// <summary>
        /// 一致する型の親要素を探索する。
        /// </summary>
        /// <typeparam name="T">探索する型</typeparam>
        /// <param name="obj"></param>
        /// <returns>[Nullable] 見つかった親要素</returns>
        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            var currentVisual = obj as Visual ?? obj.GetParentVisual();
            var parent = VisualTreeHelper.GetParent(currentVisual);

            if (parent == default)
            {
                return default;
            }
            else
            {
                return parent as T ?? parent.FindAncestor<T>();
            }
        }

        /// <summary>
        /// 指定型の子要素を列挙する。
        /// </summary>
        /// <typeparam name="T">子要素の型</typeparam>
        /// <param name="object">要素</param>
        /// <returns>子要素</returns>
        public static IEnumerable<T> EnumerateChildren<T>(this DependencyObject @object) where T : DependencyObject
        {
            if (@object == null)
            {
                yield break;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(@object);

            for (int childIdx = 0; childIdx < childCount; ++childIdx)
            {
                // 現在の要素が指定の型であればそれを列挙する
                if (VisualTreeHelper.GetChild(@object, childIdx) is T element)
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// 指定型の子要素を探索する。
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <param name="obj">要素</param>
        /// <returns>子要素</returns>
        public static T FindVisualChild<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(obj);

                for (int childIdx = 0; childIdx < childrenCount; ++childIdx)
                {
                    // 現在の要素が指定の型であればそれを返す
                    var currentChild = VisualTreeHelper.GetChild(obj, childIdx);
                    if (currentChild is T currentTChild)
                    {
                        return currentTChild;
                    }

                    // 孫要素を探索する
                    var grandChild = currentChild.FindVisualChild<T>();
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// 親要素のVisualを取得する。
        /// </summary>
        /// <param name="obj">要素</param>
        /// <returns>[Nullable] </returns>
        public static DependencyObject GetParentVisual(this DependencyObject? obj)
        {
            if (obj == default)
            {
                return default;
            }

            var parent = LogicalTreeHelper.GetParent(obj);

            if (parent == default)
            {
                return default;
            }
            else
            {
                // 親要素のVisual要素を探す
                return parent as Visual ?? parent.GetParentVisual();
            }
        }
    }
}
