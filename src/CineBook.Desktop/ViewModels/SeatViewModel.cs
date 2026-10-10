using CineBook.Desktop.Core;

namespace CineBook.Desktop.ViewModels;

// One seat in the hall. IsSelected is two-way bound to a ToggleButton.
public class SeatViewModel(int row, int number, bool isBooked) : ObservableObject
{
    private bool _isSelected;

    public int Row { get; } = row;
    public int Number { get; } = number;
    public bool IsBooked { get; } = isBooked;

    // Booked seats are disabled in the UI, so IsAvailable drives IsEnabled.
    public bool IsAvailable => !IsBooked;

    // Row 1 becomes "A", row 2 becomes "B", and so on.
    public string Label => $"{(char)('A' + Row - 1)}{Number}";

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
