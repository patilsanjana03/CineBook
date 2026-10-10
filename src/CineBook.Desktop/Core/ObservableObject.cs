using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CineBook.Desktop.Core;

// Base class for every view model. When a property changes it raises PropertyChanged,
// which is how WPF knows to refresh whatever is bound to it.
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Sets the field and notifies, but only if the value really changed.
    // Returns true when it changed, so callers can react to it.
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
