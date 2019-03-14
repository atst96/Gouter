using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class SortedNotifiableCollectionWrapper<T> : NotificationObject, IReadOnlyList<T>, INotifyCollectionChanged, IDisposable
    {
        private INotifyCollectionChanged _notifier;
        private IList<T> _collection;
        private List<T> _listImpl;
        private IComparer<T> _comparer;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public SortedNotifiableCollectionWrapper(IList<T> collection, IComparer<T> comparer)
        {
            this._notifier = collection as INotifyCollectionChanged ?? throw new NotSupportedException();
            this._collection = collection;
            this._listImpl = new List<T>();
            this._comparer = comparer;

            if (this._collection.Count > 0)
            {
                var items = this._collection.OrderBy(key => key, this._comparer);
                this._listImpl.AddRange(items);
            }

            this._notifier.CollectionChanged += this.OnCollectionChanged;
        }

        private int AddImpl(T item)
        {
            if (this._listImpl.Count == 0)
            {
                this._listImpl.Add(item);
                return 0;
            }

            for (int i = 0; i < this._listImpl.Count; ++i)
            {
                var compareItem = this._listImpl[i];

                int result = this._comparer.Compare(compareItem, item);

                if (result > 0)
                {
                    this._listImpl.Insert(i, item);
                    return i;
                }
            }

            this._listImpl.Add(item);
            return this._listImpl.Count - 1;
        }

        private void AddItems(IList items)
        {
            foreach (T item in items)
            {
                int idx = this.AddImpl(item);
                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, item, idx));
            }

            this.CountPropertyUpdated();
        }

        private void RemoveItems(IList items)
        {
            foreach (T item in items)
            {
                int idx = this._listImpl.IndexOf(item);
                if (idx < 0)
                {
                    continue;
                }

                this._listImpl.RemoveAt(idx);

                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, item, idx));
            }

            this.CountPropertyUpdated();
        }

        private void ReplaceItems(IList newItems, IList oldItems)
        {
            for (int idx = 0; idx < oldItems.Count; ++idx)
            {
                var oldItem = (T)oldItems[idx];
                var newItem = (T)newItems[idx];

                int itemIdx = this._listImpl.IndexOf(oldItem);
                if (itemIdx < 0)
                {
                    continue;
                }

                this._listImpl[itemIdx] = newItem;

                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, newItem, oldItem, itemIdx));
            }
        }

        public void ChangeComparer(IComparer<T> comparer)
        {
            if (this._comparer != comparer)
            {
                this._comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));

                this._listImpl.Clear();

                this.ResetImplList(this._collection);
            }
        }

        private void ResetImplList(IList<T> items)
        {
            this._listImpl.AddRange(items.OrderBy(key => key, this._comparer));

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, (IList)items));

            this.CountPropertyUpdated();
        }

        private void ResetItems(IList newItems)
        {
            this._listImpl.Clear();

            if (newItems.Count == 0)
            {
                this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                this.CountPropertyUpdated();
                return;
            }

            this.ResetImplList(newItems.Cast<T>().ToList());
        }

        private void CountPropertyUpdated()
        {
            this.RaisePropertyChanged(nameof(this.Count));
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.AddItems(e.NewItems);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    this.RemoveItems(e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    this.ReplaceItems(e.NewItems, e.OldItems);
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.ResetItems(e.NewItems);
                    break;
            }
        }

        public T this[int index] => this._listImpl[index];

        public int Count => this._listImpl.Count;

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            this.CollectionChanged?.Invoke(this, args);
        }

        public void Dispose()
        {
            this._notifier.CollectionChanged -= this.OnCollectionChanged;

            this._notifier = null;
            this._comparer = null;
            this._collection = null;
            this._listImpl = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _listImpl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
