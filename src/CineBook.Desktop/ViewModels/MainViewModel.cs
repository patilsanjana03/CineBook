using System.Windows.Input;
using CineBook.Desktop.Core;
using CineBook.Desktop.Models;
using CineBook.Desktop.Services;

namespace CineBook.Desktop.ViewModels;

// The "shell". It owns the single ApiClient and decides which screen is showing.
// Each screen is just another view model assigned to CurrentViewModel.
public class MainViewModel : ObservableObject
{
    private readonly ApiClient _api = new();

    private object? _currentViewModel;
    private bool _isLoggedIn;
    private string _greeting = "";

    public ICommand ShowMoviesCommand { get; }
    public ICommand ShowBookingsCommand { get; }
    public ICommand LogoutCommand { get; }

    public object? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        private set => SetProperty(ref _isLoggedIn, value);
    }

    public string Greeting
    {
        get => _greeting;
        private set => SetProperty(ref _greeting, value);
    }

    public MainViewModel()
    {
        ShowMoviesCommand = new RelayCommand(_ => ShowMovies());
        ShowBookingsCommand = new RelayCommand(_ => ShowBookings());
        LogoutCommand = new RelayCommand(_ => ShowLogin());

        ShowLogin();
    }

    private void ShowLogin()
    {
        _api.ClearToken();
        IsLoggedIn = false;
        Greeting = "";
        CurrentViewModel = new LoginViewModel(_api, OnLoggedIn);
    }

    private void OnLoggedIn(AuthResponse auth)
    {
        _api.SetToken(auth.Token);
        Greeting = $"Hi, {auth.FullName}";
        IsLoggedIn = true;
        ShowMovies();
    }

    private void ShowMovies() =>
        CurrentViewModel = new MoviesViewModel(_api, OpenSeatSelection);

    private void ShowBookings() =>
        CurrentViewModel = new MyBookingsViewModel(_api);

    // Screens never create each other directly. They get a callback instead,
    // which keeps them independent and easy to change later.
    private void OpenSeatSelection(MovieDto movie, ShowDto show) =>
        CurrentViewModel = new SeatSelectionViewModel(_api, movie, show, onBooked: ShowBookings, onBack: ShowMovies);
}
