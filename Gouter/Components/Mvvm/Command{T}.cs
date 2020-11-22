using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gouter
{
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
    }
}
