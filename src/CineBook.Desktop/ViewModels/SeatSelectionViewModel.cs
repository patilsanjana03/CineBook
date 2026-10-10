using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using CineBook.Desktop.Core;
using CineBook.Desktop.Models;
using CineBook.Desktop.Services;

namespace CineBook.Desktop.ViewModels;

public class SeatSelectionViewModel : ObservableObject
{
    private const int MaxSeats = 6;

    private readonly ApiClient _api;
    private readonly MovieDto _movie;
    private readonly ShowDto _show;
    private readonly Action _onBooked;

    private decimal _price;
    private int _seatsPerRow;
    private string _selectedSummary = "No seats selected";
    private string _errorMessage = "";
    private bool _isBusy;

    public ObservableCollection<SeatViewModel> Seats { get; } = new();

    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }

    public string Title => _movie.Title;
    public string Subtitle => _show.StartsAt.ToString("dddd, dd MMM yyyy 'at' hh:mm tt");

    // The seat grid reads this to know how many columns to draw.
    public int SeatsPerRow
    {
        get => _seatsPerRow;
        private set => SetProperty(ref _seatsPerRow, value);
    }

    public string SelectedSummary
    {
        get => _selectedSummary;
        private set => SetProperty(ref _selectedSummary, value);
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

    public SeatSelectionViewModel(ApiClient api, MovieDto movie, ShowDto show, Action onBooked, Action onBack)
    {
        _api = api;
        _movie = movie;
        _show = show;
        _onBooked = onBooked;
        _price = show.Price;

        // The button is only clickable when at least one seat is picked.
        ConfirmCommand = new AsyncRelayCommand(_ => ConfirmAsync(), _ => Seats.Any(s => s.IsSelected));
        BackCommand = new RelayCommand(_ => onBack());

        _ = LoadSeatsAsync();
    }

    private async Task LoadSeatsAsync()
    {
        IsBusy = true;

        try
        {
            var map = await _api.GetSeatMapAsync(_show.Id);
            _price = map.Price;
            SeatsPerRow = map.SeatsPerRow;

            var booked = map.BookedSeats.Select(s => (s.Row, s.Number)).ToHashSet();

            foreach (var old in Seats)
            {
                old.PropertyChanged -= OnSeatChanged;
            }
            Seats.Clear();

            // Seats are added row by row, so the grid (which wraps after SeatsPerRow
            // items) lines up as A1..A8, B1..B8, and so on.
            for (var row = 1; row <= map.Rows; row++)
            {
                for (var number = 1; number <= map.SeatsPerRow; number++)
                {
                    var seat = new SeatViewModel(row, number, booked.Contains((row, number)));
                    seat.PropertyChanged += OnSeatChanged;
                    Seats.Add(seat);
                }
            }

            UpdateSummary();
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

    private void OnSeatChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SeatViewModel.IsSelected))
        {
            return;
        }

        // Enforce the limit on the client too, so people get instant feedback.
        // (The server checks it again, because the client can never be trusted.)
        if (sender is SeatViewModel seat && seat.IsSelected && Seats.Count(s => s.IsSelected) > MaxSeats)
        {
            seat.IsSelected = false;
            ErrorMessage = $"You can book up to {MaxSeats} seats at a time.";
            return;
        }

        ErrorMessage = "";
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var chosen = Seats.Where(s => s.IsSelected).Select(s => s.Label).ToList();

        SelectedSummary = chosen.Count == 0
            ? "No seats selected"
            : $"{string.Join(", ", chosen)}  -  Total: ₹{chosen.Count * _price:N0}";
    }

    private async Task ConfirmAsync()
    {
        var chosen = Seats
            .Where(s => s.IsSelected)
            .Select(s => new SeatDto(s.Row, s.Number))
            .ToList();

        IsBusy = true;
        ErrorMessage = "";

        try
        {
            await _api.BookSeatsAsync(_show.Id, chosen);

            // A MessageBox in a view model isn't textbook MVVM (a dialog service would be),
            // but it keeps this project easy to read.
            MessageBox.Show("Your seats are booked. Enjoy the movie!", "Booking confirmed",
                MessageBoxButton.OK, MessageBoxImage.Information);

            _onBooked();
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;

            // 409 means someone else grabbed a seat first, so refresh the map to show it as taken.
            if (ex.StatusCode == HttpStatusCode.Conflict)
            {
                await LoadSeatsAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
