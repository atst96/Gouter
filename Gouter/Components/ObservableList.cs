using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Gouter;

/// <summary>
/// 変更通知コレクション
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableList<T> : IList<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private List<T> _list;
    private PropertyChangedEventArgs _countProeprtyChangedArgs = new(nameof(Count));
    private PropertyChangedEventArgs _hasItemsPropertyChangedArgs = new(nameof(Count));

    /// <summary>
    /// コレクション通知イベントハンドラ
    /// </summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>
    /// プロパティ通知イベントハンドラ
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// <see cref="ObservableList{T}"/>を作成する
    /// </summary>
    public ObservableList()
    {
        this._list = new List<T>();

        this.UpdateItemsCount();
    }

    /// <summary>
    /// <see cref="ObservableList{T}"/>を作成する
    /// </summary>
    /// <param name="capacity">リストのキャパシティ</param>
    public ObservableList(int capacity)
    {
        this._list = new List<T>(capacity);

        this.UpdateItemsCount();
    }

    /// <summary>
    /// <see cref="ObservableList{T}"/>を作成する
    /// </summary>
    /// <param name="collection">コレクション</param>
    public ObservableList(IEnumerable<T> collection)
    {
        this._list = collection?.ToList() ?? new List<T>();

        this.UpdateItemsCount();
    }

    /// <summary>
    /// <see cref="ObservableList{T}"/>を作成する
    /// </summary>
    /// <param name="collection">コレクション</param>
    /// <param name="capacity">リストのキャパシティ</param>
    public ObservableList(IEnumerable<T> collection, int capacity)
    {
        this._list = new List<T>(capacity);
        this._list.AddRange(collection);

        this.UpdateItemsCount();
    }

    /// <summary>
    /// 要素数を取得する
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// 要素の有無を取得する
    /// </summary>
    public bool HasItems { get; private set; }

    #region List impleents

    /// <summary>
    /// リストが読み取り専用かどうかを取得する
    /// </summary>
    public bool IsReadOnly { get; } = false;

    /// <summary>
    /// リスト内に要素が存在するかを取得する
    /// </summary>
    /// <param name="item">検索対象の要素</param>
    /// <returns>要素の有無</returns>
    public bool Contains(T item)
    {
        return this._list.Contains(item);
    }

    /// <summary>
    /// リストに要素をコピーする
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this._list.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// リストの<see cref="IEnumerator{T}"/>を取得する
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>
    /// リストの<see cref="IEnumerator"/>を取得する
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>
    /// 指定のインデックスの要素を取得する
    /// </summary>
    /// <param name="index">要素のインデックス</param>
    /// <returns></returns>
    public T this[int index]
    {
        get => this._list[index];
        set => this._list[index] = value;
    }

    /// <summary>
    /// 指定要素のインデックスを取得する
    /// </summary>
    /// <param name="item">検索する要素</param>
    /// <returns>要素のインデックス</returns>
    public int IndexOf(T item)
    {
        return this._list.IndexOf(item);
    }

    #endregion List implements

    /// <summary>
    /// リストに要素を追加する
    /// </summary>
    /// <param name="item">追加する要素</param>
    public void Add(T item)
    {
        this._list.Add(item);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Add, item, this._list.Count));
        this.UpdateItemsCount();
    }

    /// <summary>
    /// リストに複数の要素を追加する
    /// </summary>
    /// <param name="collection">追加する要素</param>
    public void AddRange(IEnumerable<T> collection)
    {
        var items = collection.ToList();

        this._list.AddRange(items);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Add, (IList)items, this._list.Count));
        this.UpdateItemsCount();
    }

    /// <summary>
    /// リストに複数の要素を挿入する
    /// </summary>
    /// <param name="index">挿入するインデックス</param>
    /// <param name="collection">挿入する要素</param>
    public void Insert(int index, T item)
    {
        this._list.Insert(index, item);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Add, item, index));
        this.UpdateItemsCount();
    }

    /// <summary>
    /// リストに複数の要素を挿入する
    /// </summary>
    /// <param name="index">挿入するインデックス</param>
    /// <param name="collection">挿入する要素</param>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        var items = collection.ToList();

        this._list.InsertRange(index, items);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Add, (IList)items, index));

        this.UpdateItemsCount();
    }

    /// <summary>
    /// リストから要素を削除する
    /// </summary>
    /// <param name="index">削除を開始するインデックス</param>
    /// <param name="count">削除する要素の件数</param>
    public void RemoveRange(int index, int count)
    {
        var items = this._list.GetRange(index, count);

        if (items.Count > 0)
        {
            this._list.RemoveRange(index, items.Count);

            this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Remove, items, index));
            this.UpdateItemsCount();
        }
    }

    /// <summary>
    /// リストから指定範囲の要素を取得する
    /// </summary>
    /// <param name="index">取得を解すするインデックス</param>
    /// <param name="count">取得する件数</param>
    /// <returns></returns>
    public IEnumerable<T> GetRange(int index, int count)
    {
        return this._list.GetRange(index, count);
    }

    /// <summary>
    /// リスト内の要素を移動する
    /// </summary>
    /// <param name="oldIndex">移動前のインデックス</param>
    /// <param name="newIndex">移動後のインデックス</param>
    public void Move(int oldIndex, int newIndex)
    {
        if (this.HasItems && MathEx.IsWithin(oldIndex, 0, this.Count - 1))
        {
            T item = this._list[oldIndex];
            this._list.RemoveAt(oldIndex);
            this._list.Insert(newIndex, item);

            this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            this.UpdateItemsCount();
        }
    }

    /// <summary>
    /// リスト内の要素を1件削除する
    /// </summary>
    /// <param name="item">削除する要素</param>
    /// <returns>削除の成否</returns>
    public bool Remove(T item)
    {
        int index = this._list.IndexOf(item);

        if (this._list.Remove(item))
        {
            this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, index));
            this.UpdateItemsCount();

            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// リスト内の要素を削除する
    /// </summary>
    /// <param name="index">削除する要素のインデックス</param>
    public void RemoveAt(int index)
    {
        int oldCount = this._list.Count;

        if (this.HasItems && MathEx.IsWithin(index, 0, oldCount - 1))
        {
            T item = this._list[index];
            this._list.RemoveAt(index);

            if (oldCount != this._list.Count)
            {
                this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, index));
                this.UpdateItemsCount();
            }
        }
    }

    /// <summary>
    /// リストから要素の削除を通知する
    /// </summary>
    /// <param name="item">削除する要素</param>
    private void RemoveAtImpl(T item, int index)
    {
        this._list.RemoveAt(index);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Remove, item, index));
    }

    /// <summary>
    /// リストから指定の要素をすべて削除する
    /// </summary>
    /// <param name="removeItem">削除する要素</param>
    public void RemoveAll(T item)
    {
        int index = 0;
        int count = this._list.Count;

        while (index < this._list.Count)
        {
            var _item = this._list[index];

            if (object.Equals(item, _item))
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
            this.UpdateItemsCount();
        }
    }

    /// <summary>
    /// リストから条件に当てはまる要素を削除する
    /// </summary>
    /// <param name="predicte">条件式</param>
    public void RemoveAll(Predicate<T> predicte)
    {
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
            this.UpdateItemsCount();
        }
    }

    /// <summary>
    /// リストのすべての要素を削除する
    /// </summary>
    public void Clear()
    {
        this._list.Clear();

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Reset));
        this.UpdateItemsCount();
    }

    /// <summary>
    /// コレクションの変更を通知する
    /// </summary>
    /// <param name="e"></param>
    private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        this.CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// <see cref="Count"/>、<see cref="HasItems"/>の変更を適用する
    /// </summary>
    private void OnItmesCountChanged()
    {
        if (this.PropertyChanged != null)
        {
            this.PropertyChanged(this, _countProeprtyChangedArgs);
            this.PropertyChanged(this, _hasItemsPropertyChangedArgs);
        }
    }

    /// <summary>
    /// <see cref="Count"/>、<see cref="HasItems"/>の変更を適用する
    /// </summary>
    private void UpdateItemsCount()
    {
        this.Count = this._list.Count;
        this.HasItems = this.Count > 0;

        this.OnItmesCountChanged();
    }

    /// <summary>
    /// リスト内の要素を初期化する
    /// </summary>
    /// <param name="collection"></param>
    public void Reset(IEnumerable<T> collection)
    {
        this._list.Clear();

        this._list.AddRange(collection);

        this.RaiseCollectionChanged(new(NotifyCollectionChangedAction.Reset));
        this.UpdateItemsCount();
    }
}
