using Gouter.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal abstract class ViewModelBase : INotifyPropertyChanged
    {
        private DialogService _dialogService;
        public DialogService DialogService => this._dialogService ?? (this._dialogService = new DialogService(this));

        private WindowService _windowService;
        public WindowService WindowService => this._windowService ?? (this._windowService = new WindowService(this));

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T changedValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (!object.Equals(changedValue, newValue))
            {
                return false;
            }

            changedValue = newValue;
            this.RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
