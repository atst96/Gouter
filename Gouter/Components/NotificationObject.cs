using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            if (object.Equals(changedValue, newValue))
            {
                return false;
            }

            changedValue = newValue;
            this.RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
