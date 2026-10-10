using System.Windows.Input;

namespace CineBook.Desktop.Core;

// Lets a view model expose an action as a command that buttons can bind to.
public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    // Tying into CommandManager means WPF re-checks CanExecute automatically
    // (for example, greying out a button) whenever the user interacts with the UI.
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => execute(parameter);
}
