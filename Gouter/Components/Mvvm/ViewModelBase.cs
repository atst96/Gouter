using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gouter.Components.Mvvm;

namespace Gouter;

/// <summary>
/// ViewModelのベースクラス
/// </summary>
internal abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// プロパティ変更通知イベントハンドラ
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// コマンド管理
    /// </summary>
    protected MvvmCommandManager Commands { get; } = new();

    /// <summary>
    /// プロパティの変更通知を行う
    /// </summary>
    /// <param name="propertyName">プロパティ名</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.PropertyChanged?.Invoke(this, new(propertyName));
    }

    /// <summary>
    /// インスタンスを破棄する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="changedValue"></param>
    /// <param name="newValue"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetProperty<T>(ref T changedValue, T newValue, [CallerMemberName] string propertyName = "")
    {
        if (object.Equals(changedValue, newValue))
        {
            return false;
        }

        changedValue = newValue;
        this.RaisePropertyChanged(propertyName);

        return true;
    }

    /// <summary>
    /// インスタンス破棄時
    /// </summary>
    protected virtual void OnDispose()
    {
        this.Commands.Dispose();
    }

    /// <summary>
    /// インスタンス破棄時
    /// </summary>
    ~ViewModelBase()
    {
        this.OnDispose();
    }

    /// <summary>
    /// インスタンスを破棄する
    /// </summary>
    void IDisposable.Dispose()
    {
        this.OnDispose();
        GC.SuppressFinalize(this);
    }
}
