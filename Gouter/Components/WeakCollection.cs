using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    class WeakCollection<T> : ICollection<WeakReference<T>>
        where T : class
    {
        private IList<WeakReference<T>> _weakItems = new List<WeakReference<T>>();

        public int Count => this._weakItems.Count;

        public bool IsReadOnly => this._weakItems.IsReadOnly;

        public void Add(T item)
        {
            this.Add(new WeakReference<T>(item));
        }

        public void Add(WeakReference<T> item)
        {
            this._weakItems.Add(item);
        }

        public void Clear()
        {
            this._weakItems.Clear();
        }

        public bool Contains(WeakReference<T> item)
        {
            return this._weakItems.Contains(item);
        }

        public void CopyTo(WeakReference<T>[] array, int arrayIndex)
        {
            this._weakItems.CopyTo(array, arrayIndex);
        }

        public IEnumerator<WeakReference<T>> GetEnumerator()
        {
            return this._weakItems.GetEnumerator();
        }

        public bool Remove(T item)
        {
            bool result = false;

            for (int i = this._weakItems.Count - 1; i >= 0; --i)
            {
                var weakItem = this._weakItems[i];

                if (this._weakItems[i].TryGetTarget(out var targetItem))
                {
                    if (targetItem.Equals(item))
                    {
                        this._weakItems.RemoveAt(i);
                        result = true;
                    }
                }
                else
                {
                    this._weakItems.RemoveAt(i);
                }
            }

            return result;
        }

        public bool Remove(WeakReference<T> item)
        {
            return item.TryGetTarget(out var target) ? this.Remove(target) : false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._weakItems.GetEnumerator();
        }
    }
}
