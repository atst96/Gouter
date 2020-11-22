using System;
using System.Windows.Input;

namespace Gouter
{
    internal abstract class Command : IDisposableCommand
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

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public void RaiseCanExecuteChanged()
        {
            this._commandCanExecuteChanged.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            this._weakHandlers.Clear();
        }
    }
}
