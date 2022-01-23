using System;
using System.Windows.Input;
using Gouter.Components.Mvvm;

namespace Gouter;

/// <summary>
/// コマンド
/// </summary>
internal abstract class Command : IDisposableCommand
{
    private readonly bool _hookRequerySuggested;
    private EventHandler _commandCanExecuteChanged;
    private readonly WeakCollection<EventHandler> _weakHandlers;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    protected Command()
    {
        this._weakHandlers = new WeakCollection<EventHandler>();
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="hookRequerySuggested"><see cref="ICommand.CanExecuteChanged"/>を<see cref="CommandManager.RequerySuggested"/>に登録するか否かのフラグ</param>
    protected Command(bool hookRequerySuggested)
    {
        this._hookRequerySuggested = hookRequerySuggested;
        this._weakHandlers = new WeakCollection<EventHandler>();
    }

    /// <summary>
    /// <see cref="ICommand.CanExecuteChanged"/>
    /// </summary>
    event EventHandler ICommand.CanExecuteChanged
    {
        add
        {
            this._commandCanExecuteChanged += value;

            if (this._hookRequerySuggested)
            {
                CommandManager.RequerySuggested += value;
            }

            this._weakHandlers.Add(new WeakReference<EventHandler>(value));
        }
        remove
        {
            this._commandCanExecuteChanged -= value;

            if (this._hookRequerySuggested)
            {
                CommandManager.RequerySuggested -= value;
            }

            this._weakHandlers.Remove(value);
        }
    }

    /// <summary>
    /// <see cref="ICommand.CanExecute(object?)"/>
    /// </summary>
    /// <param name="parameter">コマンド実行時のパラメータ</param>
    /// <returns></returns>
    public abstract bool CanExecute(object parameter);

    /// <summary>
    /// <see cref="ICommand.Execute(object?)"/>
    /// </summary>
    /// <param name="parameter">コマンド実行時のパラメータ</param>
    public abstract void Execute(object parameter);

    /// <summary>
    /// <see cref="ICommand.CanExecuteChanged"/>を発火させる
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        this._commandCanExecuteChanged.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// インスタンスを破棄する
    /// </summary>
    public virtual void Dispose()
    {
        this._weakHandlers.Clear();
    }

    #region Create command

    /// <summary>
    /// コマンドを作成する
    /// </summary>
    /// <param name="execute"></param>
    /// <returns></returns>
    public static Command Create(Action execute) => new DelegateCommand(execute);

    /// <summary>
    /// コマンドを作成する
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="hookRequerySuggested"></param>
    /// <returns></returns>
    public static Command Create(Action execute, bool hookRequerySuggested)
        => new DelegateCommand(execute, hookRequerySuggested);

    /// <summary>
    /// コマンドを作成する
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    /// <returns></returns>
    public static Command Create(Action execute, Func<bool> canExecute)
        => new DelegateCommand(execute, canExecute);

    /// <summary>
    /// コマンドを作成する
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    /// <param name="hookRequerySuggested"></param>
    /// <returns></returns>
    public static Command Create(Action execute, Func<bool> canExecute, bool hookRequerySuggested)
        => new DelegateCommand(execute, canExecute, hookRequerySuggested);

    #endregion Create command
}
