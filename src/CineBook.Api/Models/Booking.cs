namespace CineBook.Api.Models;

// One booked seat. Booking three seats creates three rows, which keeps
// cancelling a single seat and checking for clashes very simple.
public class Booking
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ShowId { get; set; }
    public Show Show { get; set; } = null!;

    // Both are 1-based. Row 1 is shown as "A" in the UI.
    public int SeatRow { get; set; }
    public int SeatNumber { get; set; }

    public DateTime BookedAt { get; set; }
}
