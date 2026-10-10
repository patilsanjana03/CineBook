namespace CineBook.Api.Services;

public static class SeatLabel
{
    // Row 1, seat 5 becomes "A5"; row 2, seat 3 becomes "B3".
    public static string From(int row, int number) => $"{(char)('A' + row - 1)}{number}";
}
