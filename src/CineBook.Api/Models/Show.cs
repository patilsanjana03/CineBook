namespace CineBook.Api.Models;

// One screening of a movie at a particular time.
// The hall layout (rows x seats per row) lives here so every show can differ.
public class Show
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public DateTime StartsAt { get; set; }
    public decimal Price { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }

    public List<Booking> Bookings { get; set; } = new();
}
