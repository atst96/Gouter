using System;
using System.Windows.Input;
using Gouter.Components.Mvvm;

namespace Gouter;

internal abstract class Command<T> : IDisposableCommand
{
    private readonly bool _hookRequerySuggested;
    private EventHandler _commandCanExecuteChanged;
    private readonly WeakCollection<EventHandler> _weakHandlers;

    protected Command()
    {
        this._weakHandlers = new WeakCollection<EventHandler>();
    }

    protected Command(bool hookRequerySuggested)
    {
        this._hookRequerySuggested = hookRequerySuggested;
        this._weakHandlers = new WeakCollection<EventHandler>();
    }

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

    bool ICommand.CanExecute(object parameter)
    {
        return this.CanExecute(parameter is T obj ? obj : default(T));
    }

    void ICommand.Execute(object parameter)
    {
        this.Execute(parameter is T obj ? obj : default(T));
    }

    public abstract bool CanExecute(T parameter);

    public abstract void Execute(T parameter);

    public void RaiseCanExecuteChanged()
    {
        this._commandCanExecuteChanged.Invoke(this, EventArgs.Empty);
    }

    public virtual void Dispose()
    {
        this._weakHandlers.Clear();
    }

    #region Create command

    /// <summary>
    /// コマンド生成
    /// </summary>
    /// <param name="execute"></param>
    /// <returns></returns>
    public static Command<T> Create(Action<T> execute) => new DelegateCommand<T>(execute);

    /// <summary>
    /// コマンド生成
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="hookRequerySuggested"></param>
    /// <returns></returns>
    public static Command<T> Create(Action<T> execute, bool hookRequerySuggested)
        => new DelegateCommand<T>(execute, hookRequerySuggested);

    /// <summary>
    /// コマンド生成
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    /// <returns></returns>
    public static Command<T> Create(Action<T> execute, Predicate<T> canExecute)
        => new DelegateCommand<T>(execute, canExecute);

    /// <summary>
    /// コマンド生成
    /// </summary>
    /// <param name="execute"></param>
    /// <param name="canExecute"></param>
    /// <param name="hookRequerySuggested"></param>
    /// <returns></returns>
    public static Command<T> Create(Action<T> execute, Predicate<T> canExecute, bool hookRequerySuggested)
        => new DelegateCommand<T>(execute, canExecute, hookRequerySuggested);

    #endregion Create command
}
