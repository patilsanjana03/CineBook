using System.Collections.ObjectModel;
using System.Windows.Input;
using CineBook.Desktop.Core;
using CineBook.Desktop.Models;
using CineBook.Desktop.Services;

namespace CineBook.Desktop.ViewModels;

// Left side: list of movies. Right side: upcoming shows for the selected movie.
public class MoviesViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private readonly Action<MovieDto, ShowDto> _openSeats;

    private MovieDto? _selectedMovie;
    private string _statusText = "Loading movies...";

    // ObservableCollection tells the UI when items are added or removed.
    public ObservableCollection<MovieDto> Movies { get; } = new();
    public ObservableCollection<ShowDto> Shows { get; } = new();

    public ICommand SelectShowCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public MovieDto? SelectedMovie
    {
        get => _selectedMovie;
        set
        {
            if (SetProperty(ref _selectedMovie, value))
            {
                // Fire and forget is fine here: LoadShowsAsync handles its own errors.
                _ = LoadShowsAsync(value);
            }
        }
    }

    public MoviesViewModel(ApiClient api, Action<MovieDto, ShowDto> openSeats)
    {
        _api = api;
        _openSeats = openSeats;

        SelectShowCommand = new RelayCommand(parameter =>
        {
            if (parameter is ShowDto show && SelectedMovie is not null)
            {
                _openSeats(SelectedMovie, show);
            }
        });

        _ = LoadMoviesAsync();
    }

    private async Task LoadMoviesAsync()
    {
        try
        {
            var movies = await _api.GetMoviesAsync();
            foreach (var movie in movies)
            {
                Movies.Add(movie);
            }

            // Select the first movie so the right-hand side isn't empty on arrival.
            SelectedMovie = Movies.FirstOrDefault();
            if (SelectedMovie is null)
            {
                StatusText = "No movies are showing right now.";
            }
        }
        catch (ApiException ex)
        {
            StatusText = ex.Message;
        }
    }

    private async Task LoadShowsAsync(MovieDto? movie)
    {
        Shows.Clear();

        if (movie is null)
        {
            StatusText = "Pick a movie to see its showtimes.";
            return;
        }

        StatusText = "Loading showtimes...";

        try
        {
            var shows = await _api.GetShowsAsync(movie.Id);

            // If the user clicked another movie while we were waiting, this answer is stale.
            if (SelectedMovie?.Id != movie.Id)
            {
                return;
            }

            foreach (var show in shows)
            {
                Shows.Add(show);
            }

            StatusText = Shows.Count == 0 ? "No upcoming shows for this movie." : $"{Shows.Count} upcoming shows";
        }
        catch (ApiException ex)
        {
            StatusText = ex.Message;
        }
    }
}
