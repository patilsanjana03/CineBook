using System.Windows.Input;
using CineBook.Desktop.Core;
using CineBook.Desktop.Models;
using CineBook.Desktop.Services;

namespace CineBook.Desktop.ViewModels;

// One screen that works as both "sign in" and "create account".
public class LoginViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly Action<AuthResponse> _onSuccess;

    private string _fullName = "";
    private string _email = "";
    private string _errorMessage = "";
    private bool _isRegisterMode;
    private bool _isBusy;

    public ICommand SubmitCommand { get; }
    public ICommand ToggleModeCommand { get; }

    // A PasswordBox can't be data-bound for security reasons, so LoginView.xaml.cs
    // copies its text into this property whenever it changes.
    public string Password { get; set; } = "";

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        private set
        {
            if (SetProperty(ref _isRegisterMode, value))
            {
                // The texts below depend on the mode, so tell the UI to re-read them.
                OnPropertyChanged(nameof(Subtitle));
                OnPropertyChanged(nameof(SubmitText));
                OnPropertyChanged(nameof(ToggleText));
            }
        }
    }

    public string Subtitle => IsRegisterMode ? "Create your account" : "Sign in to book your seats";
    public string SubmitText => IsRegisterMode ? "Create account" : "Sign in";
    public string ToggleText => IsRegisterMode ? "I already have an account" : "New here? Create an account";

    public LoginViewModel(ApiClient api, Action<AuthResponse> onSuccess)
    {
        _api = api;
        _onSuccess = onSuccess;

        SubmitCommand = new AsyncRelayCommand(_ => SubmitAsync());
        ToggleModeCommand = new RelayCommand(_ =>
        {
            IsRegisterMode = !IsRegisterMode;
            ErrorMessage = "";
        });
    }

    private async Task SubmitAsync()
    {
        IsBusy = true;
        ErrorMessage = "";

        try
        {
            var result = IsRegisterMode
                ? await _api.RegisterAsync(FullName, Email, Password)
                : await _api.LoginAsync(Email, Password);

            _onSuccess(result);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
