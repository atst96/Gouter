using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gouter
{
    public class ConcurrentNotifiableCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<T> _list;
        private readonly object _synchronizeObject = new object();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ConcurrentNotifiableCollection()
        {
            this._list = new List<T>();

            this.ApplyItemsCount();
        }

        public ConcurrentNotifiableCollection(int capacity)
        {
            this._list = new List<T>(capacity);

            this.ApplyItemsCount();
        }

        public ConcurrentNotifiableCollection(IEnumerable<T> collection)
        {
            this._list = collection?.ToList() ?? new List<T>();

            this.ApplyItemsCount();
        }

        public ConcurrentNotifiableCollection(IEnumerable<T> collection, int capacity)
        {
            this._list = new List<T>(capacity);
            this._list.AddRange(collection);

            this.ApplyItemsCount();
        }

        public int Count { get; private set; }

        public bool HasItems { get; private set; }

        #region Bein: List impleents

        public bool IsReadOnly { get; } = false;

        public bool Contains(T item)
        {
            return this._list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public T this[int index]
        {
            get => this._list[index];
            set => this._list[index] = value;
        }

        public int IndexOf(T item)
        {
            return this._list.IndexOf(item);
        }

        #endregion End: List implements

        public void Add(T item)
        {
            Monitor.Enter(this._synchronizeObject);

            this._list.Add(item);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            Monitor.Enter(this._synchronizeObject);

            var items = NormalizeCollection(collection);

            this._list.AddRange(items);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, (IList)items));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }

        public void Insert(int index, T item)
        {
            Monitor.Enter(this._synchronizeObject);

            this._list.Insert(index, item);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, item, index));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            Monitor.Enter(this._synchronizeObject);

            var items = NormalizeCollection(collection);

            this._list.InsertRange(index, items);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, (IList)items, index));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }

        public void RemoveRange(int index, int count)
        {
            Monitor.Enter(this._synchronizeObject);

            var items = this._list.GetRange(index, count);

            if (items.Count > 0)
            {
                this._list.RemoveRange(index, items.Count);

                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, items, index));

                this.ApplyItemsCount();
            }

            Monitor.Exit(this._synchronizeObject);
        }

        public IEnumerable<T> GetRange(int index, int count)
        {
            Monitor.Enter(this._synchronizeObject);

            var results = this._list.GetRange(index, count);

            Monitor.Exit(this._synchronizeObject);

            return results;
        }

        public void Move(int oldIndex, int newIndex)
        {
            Monitor.Exit(this._synchronizeObject);

            if (this.HasItems && MathEx.IsWithin(oldIndex, 0, this.Count - 1))
            {
                T item = this._list[oldIndex];
                this._list.RemoveAt(oldIndex);
                this._list.Insert(newIndex, item);

                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Move,
                        item, newIndex, oldIndex));

                this.ApplyItemsCount();
            }

            Monitor.Exit(this._synchronizeObject);
        }

        public bool Remove(T item)
        {
            Monitor.Enter(this._synchronizeObject);

            int index = this._list.IndexOf(item);

            bool result = false;

            if (this._list.Remove(item))
            {
                this.RaiseCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove, item, index));

                this.ApplyItemsCount();

                result = true;
            }

            Monitor.Exit(this._synchronizeObject);

            return result;
        }

        public void RemoveAt(int index)
        {
            Monitor.Enter(this._synchronizeObject);

            int oldCount = this._list.Count;

            if (this.HasItems && MathEx.IsWithin(index, 0, oldCount - 1))
            {
                T item = this._list[index];
                this._list.RemoveAt(index);

                if (oldCount != this._list.Count)
                {
                    this.RaiseCollectionChanged(
                        new NotifyCollectionChangedEventArgs(
                            NotifyCollectionChangedAction.Remove, item, index));

                    this.ApplyItemsCount();
                }
            }

            Monitor.Exit(this._synchronizeObject);
        }

        private void RemoveAtImpl(T item, int index)
        {
            Monitor.Enter(this._synchronizeObject);

            this._list.RemoveAt(index);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove, item, index));

            Monitor.Exit(this._synchronizeObject);
        }

        public void RemoveAll(T removeItem)
        {
            Monitor.Enter(this._synchronizeObject);

            int index = 0;
            int count = this._list.Count;

            while (index < this._list.Count)
            {
                var currentItem = this._list[index];

                if (object.Equals(removeItem, currentItem))
                {
                    this.RemoveAtImpl(currentItem, index);
                }
                else
                {
                    ++index;
                }
            }

            if (this._list.Count != count)
            {
                this.ApplyItemsCount();
            }

            Monitor.Exit(this._synchronizeObject);
        }

        public void RemoveAll(Predicate<T> predicte)
        {
            Monitor.Enter(this._synchronizeObject);

            int index = 0;
            int count = this._list.Count;

            while (index < this._list.Count)
            {
                var _item = this._list[index];

                if (predicte.Invoke(_item))
                {
                    this.RemoveAtImpl(_item, index);
                }
                else
                {
                    ++index;
                }
            }

            if (this._list.Count != count)
            {
                this.ApplyItemsCount();
            }

            Monitor.Exit(this._synchronizeObject);
        }

        public void Clear()
        {
            Monitor.Enter(this._synchronizeObject);

            this._list.Clear();

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }

        private void OnItmesCountChanged()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.Count)));
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.HasItems)));
            }
        }

        private void ApplyItemsCount()
        {
            this.Count = this._list.Count;
            this.HasItems = this.Count > 0;

            this.OnItmesCountChanged();
        }

        private static List<T> NormalizeCollection(IEnumerable<T> collection)
        {
            return collection.ToList();
        }

        public void Reset() => this.Clear();

        public void Reset(IEnumerable<T> collection)
        {
            Monitor.Enter(this._synchronizeObject);

            this._list.Clear();

            this._list.AddRange(collection);

            this.RaiseCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));

            this.ApplyItemsCount();

            Monitor.Exit(this._synchronizeObject);
        }
    }
}