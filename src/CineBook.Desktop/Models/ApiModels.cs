namespace CineBook.Desktop.Models;

// Mirrors the DTOs of the API. The property names must match the JSON the server sends.

public record AuthResponse(string Token, string FullName, string Email, string Role);

public record MovieDto(int Id, string Title, string Description, string Genre, int DurationMinutes);

public record ShowDto(int Id, int MovieId, DateTime StartsAt, decimal Price, int Rows, int SeatsPerRow);

public record SeatDto(int Row, int Number);

public record SeatMapDto(int ShowId, int Rows, int SeatsPerRow, decimal Price, List<SeatDto> BookedSeats);

public record BookSeatsRequest(int ShowId, List<SeatDto> Seats);

public record BookingDto(int Id, string MovieTitle, DateTime StartsAt, string SeatLabel, decimal Price, DateTime BookedAt);
