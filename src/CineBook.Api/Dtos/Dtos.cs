namespace CineBook.Api.Dtos;

// DTOs are the shapes we send/receive over HTTP. Keeping them separate from the
// database models means we never leak things like password hashes by accident.

public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string FullName, string Email, string Role);

// Every error the API returns has this shape, so the desktop app only needs one parser.
public record ErrorResponse(string Message);

public record MovieDto(int Id, string Title, string Description, string Genre, int DurationMinutes);
public record CreateMovieRequest(string Title, string Description, string Genre, int DurationMinutes);

public record ShowDto(int Id, int MovieId, DateTime StartsAt, decimal Price, int Rows, int SeatsPerRow);
public record CreateShowRequest(int MovieId, DateTime StartsAt, decimal Price, int Rows, int SeatsPerRow);

public record SeatDto(int Row, int Number);
public record SeatMapDto(int ShowId, int Rows, int SeatsPerRow, decimal Price, List<SeatDto> BookedSeats);

public record BookSeatsRequest(int ShowId, List<SeatDto> Seats);
public record BookingDto(int Id, string MovieTitle, DateTime StartsAt, string SeatLabel, decimal Price, DateTime BookedAt);
