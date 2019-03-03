using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal abstract class NotificationObject : INotifyPropertyChanged
    {
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
