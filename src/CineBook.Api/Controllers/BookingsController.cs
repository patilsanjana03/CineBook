using System.Security.Claims;
using CineBook.Api.Data;
using CineBook.Api.Dtos;
using CineBook.Api.Models;
using CineBook.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBook.Api.Controllers;

// Everything here needs a valid token.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController(AppDbContext db) : ControllerBase
{
    private const int MaxSeatsPerBooking = 6;

    // The user id comes from the token, never from the request body. Otherwise someone
    // could book or cancel on behalf of another user just by changing a number.
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<List<BookingDto>>> Book(BookSeatsRequest request)
    {
        if (request.Seats.Count == 0)
            return BadRequest(new ErrorResponse("Pick at least one seat."));
        if (request.Seats.Count > MaxSeatsPerBooking)
            return BadRequest(new ErrorResponse($"You can book up to {MaxSeatsPerBooking} seats at a time."));

        var show = await db.Shows.Include(s => s.Movie).FirstOrDefaultAsync(s => s.Id == request.ShowId);
        if (show is null)
            return NotFound(new ErrorResponse("Show not found."));
        if (show.StartsAt <= DateTime.Now)
            return BadRequest(new ErrorResponse("This show has already started."));

        // Records compare by value, so Distinct() removes repeated seats.
        var seats = request.Seats.Distinct().ToList();
        if (seats.Count != request.Seats.Count)
            return BadRequest(new ErrorResponse("The same seat was selected twice."));

        var outsideHall = seats.Any(s =>
            s.Row < 1 || s.Row > show.Rows || s.Number < 1 || s.Number > show.SeatsPerRow);
        if (outsideHall)
            return BadRequest(new ErrorResponse("One of the seats does not exist in this hall."));

        // Friendly check first, so the common case gets a clear message...
        var taken = await db.Bookings
            .Where(b => b.ShowId == show.Id)
            .Select(b => new { b.SeatRow, b.SeatNumber })
            .ToListAsync();

        var clash = seats.Any(s => taken.Any(t => t.SeatRow == s.Row && t.SeatNumber == s.Number));
        if (clash)
            return Conflict(new ErrorResponse("Some of those seats were just taken. Please pick again."));

        var bookings = seats.Select(s => new Booking
        {
            UserId = CurrentUserId,
            ShowId = show.Id,
            SeatRow = s.Row,
            SeatNumber = s.Number,
            BookedAt = DateTime.Now
        }).ToList();

        db.Bookings.AddRange(bookings);

        try
        {
            // SaveChanges runs as one transaction: either every seat is booked or none is.
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // ...and the unique index is the safety net if two people raced past the check above.
            return Conflict(new ErrorResponse("Some of those seats were just taken. Please pick again."));
        }

        return Ok(bookings.Select(b => ToDto(b, show)).ToList());
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<BookingDto>>> GetMine()
    {
        var userId = CurrentUserId;

        var bookings = await db.Bookings
            .AsNoTracking()
            .Include(b => b.Show)
            .ThenInclude(s => s.Movie)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Show.StartsAt)
            .ToListAsync();

        return Ok(bookings.Select(b => ToDto(b, b.Show)).ToList());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = CurrentUserId;
        var booking = await db.Bookings.Include(b => b.Show).FirstOrDefaultAsync(b => b.Id == id);

        // Someone else's booking looks exactly like a missing one, so ids can't be probed.
        if (booking is null || booking.UserId != userId)
            return NotFound(new ErrorResponse("Booking not found."));
        if (booking.Show.StartsAt <= DateTime.Now)
            return BadRequest(new ErrorResponse("You can't cancel a show that has already started."));

        db.Bookings.Remove(booking); // removing the row frees the seat again
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static BookingDto ToDto(Booking b, Show show) =>
        new(b.Id, show.Movie.Title, show.StartsAt, SeatLabel.From(b.SeatRow, b.SeatNumber), show.Price, b.BookedAt);
}
