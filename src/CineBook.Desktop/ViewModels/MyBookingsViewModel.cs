using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CineBook.Desktop.Core;
using CineBook.Desktop.Models;
using CineBook.Desktop.Services;

namespace CineBook.Desktop.ViewModels;

public class MyBookingsViewModel : ObservableObject
{
    private readonly ApiClient _api;
    private string _statusText = "Loading your bookings...";

    public ObservableCollection<BookingDto> Bookings { get; } = new();

    public ICommand CancelCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public MyBookingsViewModel(ApiClient api)
    {
        _api = api;
        CancelCommand = new AsyncRelayCommand(parameter => CancelAsync(parameter as BookingDto));

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var bookings = await _api.GetMyBookingsAsync();

            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }

            StatusText = Bookings.Count == 0 ? "You have no bookings yet." : $"{Bookings.Count} booked seats";
        }
        catch (ApiException ex)
        {
            StatusText = ex.Message;
        }
    }

    private async Task CancelAsync(BookingDto? booking)
    {
        if (booking is null)
        {
            return;
        }

        var answer = MessageBox.Show(
            $"Cancel seat {booking.SeatLabel} for {booking.MovieTitle}?",
            "Cancel booking", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _api.CancelBookingAsync(booking.Id);
            await LoadAsync();
        }
        catch (ApiException ex)
        {
            MessageBox.Show(ex.Message, "Could not cancel", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
