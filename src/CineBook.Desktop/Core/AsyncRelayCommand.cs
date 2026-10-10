using System.Windows;
using System.Windows.Input;

namespace CineBook.Desktop.Core;

// Same idea as RelayCommand, but for actions that await something (like an API call).
// While it's running the button is disabled, which stops double clicks sending two requests.
public class AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    private bool _isRunning;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) =>
        !_isRunning && (canExecute?.Invoke(parameter) ?? true);

    // "async void" is normally a smell, but ICommand.Execute forces a void signature.
    // The try/catch means an unexpected error can't crash the whole app.
    public async void Execute(object? parameter)
    {
        _isRunning = true;
        CommandManager.InvalidateRequerySuggested();

        try
        {
            await execute(parameter);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Something went wrong: {ex.Message}", "CineBook",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isRunning = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
