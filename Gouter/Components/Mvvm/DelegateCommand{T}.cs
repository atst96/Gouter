using System;

namespace Gouter.Components.Mvvm;

internal class DelegateCommand<T> : Command<T>
{
    private Action<T> _execute;
    private Predicate<T> _canExecute;

    public DelegateCommand(Action<T> execute)
        : this(execute, EmptyCanExecute, false)
    {
    }

    public DelegateCommand(Action<T> execute, bool hookRequerySuggested)
        : this(execute, EmptyCanExecute, hookRequerySuggested)
    {
    }

    public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        : this(execute, canExecute, false)
    {
    }

    public DelegateCommand(Action<T> execute, Predicate<T> canExecute, bool hookRequerySuggested)
        : base(hookRequerySuggested)
    {
        this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this._canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
    }

    public override bool CanExecute(T parameter)
    {
        return this._canExecute.Invoke(parameter);
    }

    public override void Execute(T parameter)
    {
        this._execute.Invoke(parameter);
    }

    public override void Dispose()
    {
        this._execute = null;
        this._canExecute = null;

        base.Dispose();
    }

    public static bool EmptyCanExecute(T parameter) => true;
}
