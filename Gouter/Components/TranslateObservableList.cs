using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Gouter.Components;

internal abstract class TranslateObservableList<TPrev, TNext>
    : IReadOnlyList<TNext>, INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
{
    /// <summary>プロパティ変更通知</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>コレクション変更通知</summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>内部リスト</summary>
    private readonly List<TNext> _list;

    /// <summary>リスト件数を取得する</summary>
    public int Count => this._list.Count;

    /** 購読先コレクション */
    private readonly INotifyCollectionChanged _baseCollection;

    /// <summary>
    /// リスト内の要素を取得する
    /// </summary>
    /// <param name="index">要素のインデックス</param>
    /// <returns>変換後の要素</returns>
    public TNext this[int index] => this._list[index];

    /// <summary>
    /// </summary>
    /// <param name="collection">監視対象のコレクション</param>
    public TranslateObservableList(ObservableList<TPrev> collection)
    {
        this._list = new(collection.Select(this.ConvertTo));
        this._baseCollection = collection;
        this._baseCollection.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    /// </summary>
    /// <param name="collection">監視対象のコレクション</param>
    public TranslateObservableList(ObservableCollection<TPrev> collection)
    {
        this._list = new(collection.Select(this.ConvertTo));
        this._baseCollection = collection;
        this._baseCollection.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    /// 要素を変換する
    /// </summary>
    /// <param name="object">変換前の要素</param>
    /// <returns>変換後の要素</returns>
    protected abstract TNext ConvertTo(TPrev @object);

    /// <summary>
    /// <see cref="IEnumerable{TNext}"/>を取得する
    /// </summary>
    public IEnumerator<TNext> GetEnumerator() => this._list.GetEnumerator();

    /// <summary>
    /// <see cref="IEnumerator"/>を取得する
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator() => this._list.GetEnumerator();

    /// <summary>
    /// コレクション変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
#pragma warning disable IDE0004 // IListのキャスト不要の提案を無効にする
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                this._list.ForEach(this.OnRemoved);
                this._list.Clear();
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
                this.OnCountPropertyChanged();
                return;

            case NotifyCollectionChangedAction.Add:
                var addItems = e.NewItems.Cast<TPrev>().Select(this.ConvertTo).ToList();
                this._list.InsertRange(e.NewStartingIndex, addItems);
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Add, (IList)addItems, e.NewStartingIndex));
                this.OnCountPropertyChanged();
                return;

            case NotifyCollectionChangedAction.Remove:
                var removeList = this._list.GetRange(e.OldStartingIndex, e.OldItems.Count);
                removeList.ForEach(this.OnRemoved);
                this._list.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Remove, (IList)removeList, e.OldStartingIndex));
                this.OnCountPropertyChanged();
                return;

            case NotifyCollectionChangedAction.Replace:
                var replaceItems = e.NewItems.Cast<TPrev>().Select(this.ConvertTo).ToList();
                for (int i = 0, idx = e.NewStartingIndex; i < e.NewItems.Count; ++i, ++idx)
                {
                    var oldItem = this._list[idx];
                    this._list[idx] = replaceItems[i];

                    if (!this._list.Contains(oldItem))
                    {
                        this.OnRemoved(oldItem);
                    }
                }
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Replace, (IList)replaceItems, e.NewStartingIndex));
                return;

            case NotifyCollectionChangedAction.Move:
                var moveItems = this._list.GetRange(e.OldStartingIndex, e.OldItems.Count);
                this._list.InsertRange(e.NewStartingIndex, moveItems);
                this.OnCollectionChanged(new(NotifyCollectionChangedAction.Move, moveItems, e.NewStartingIndex, e.OldStartingIndex));
                return;
        }
#pragma warning restore IDE0004
    }

    private static PropertyChangedEventArgs CountProeprty { get; } = new PropertyChangedEventArgs(nameof(Count));

    /// <summary>
    /// <see cref="Count"/>プロパティの変更通知
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnCountPropertyChanged()
    {
        this.PropertyChanged?.Invoke(this, CountProeprty);
    }

    /// <summary>
    /// コレクションの変更通知
    /// </summary>
    /// <param name="e"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        this.CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// 要素削除時
    /// </summary>
    /// <param name="item">変換後の要素</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void OnRemoved(TNext @item) { }

    private bool _isDisposed;

    /// <summary>
    /// インスタンス破棄時
    /// </summary>
    ~TranslateObservableList() => ((IDisposable)this).Dispose();

    /// <summary>
    /// インスタンスの破棄
    /// </summary>
    void IDisposable.Dispose()
    {
        if (this._isDisposed)
        {
            return;
        }

        this._baseCollection.CollectionChanged -= this.OnCollectionChanged;

        this._isDisposed = true;
    }
}
