using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gouter.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        bool isChanged = !EqualityComparer<T>.Default.Equals(field, value);
        if (!isChanged)
        {
            field = value;
            this.OnPropertyChanged(propertyName);
        }

        return isChanged;
    }

    protected bool SetIfChanged<T>(ref T field, ref T value, [CallerMemberName] string propertyName = "")
    {
        bool isChanged = !EqualityComparer<T>.Default.Equals(field, value);
        if (!isChanged)
        {
            field = value;
            this.OnPropertyChanged(propertyName);
        }

        return isChanged;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void OnPropertyChanged(string? propertyName)
    {
        this.PropertyChanged?.Invoke(this, new(propertyName));
    }
}
