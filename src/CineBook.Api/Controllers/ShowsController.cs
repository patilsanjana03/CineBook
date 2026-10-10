using CineBook.Api.Data;
using CineBook.Api.Dtos;
using CineBook.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBook.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowsController(AppDbContext db) : ControllerBase
{
    // Upcoming shows for one movie, earliest first.
    [HttpGet("movie/{movieId:int}")]
    public async Task<ActionResult<List<ShowDto>>> GetForMovie(int movieId)
    {
        var now = DateTime.Now;

        var shows = await db.Shows
            .AsNoTracking()
            .Where(s => s.MovieId == movieId && s.StartsAt > now)
            .OrderBy(s => s.StartsAt)
            .ToListAsync();

        return Ok(shows.Select(ToDto).ToList());
    }

    // The seat map only lists seats that are already taken; the client works out the rest
    // from Rows x SeatsPerRow. That keeps the response small.
    [HttpGet("{id:int}/seats")]
    public async Task<ActionResult<SeatMapDto>> GetSeats(int id)
    {
        var show = await db.Shows.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (show is null)
            return NotFound(new ErrorResponse("Show not found."));

        var booked = await db.Bookings
            .AsNoTracking()
            .Where(b => b.ShowId == id)
            .Select(b => new SeatDto(b.SeatRow, b.SeatNumber))
            .ToListAsync();

        return Ok(new SeatMapDto(show.Id, show.Rows, show.SeatsPerRow, show.Price, booked));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShowDto>> Create(CreateShowRequest request)
    {
        if (!await db.Movies.AnyAsync(m => m.Id == request.MovieId))
            return BadRequest(new ErrorResponse("That movie does not exist."));
        if (request.Price <= 0)
            return BadRequest(new ErrorResponse("Price must be more than zero."));
        if (request.Rows is < 1 or > 20 || request.SeatsPerRow is < 1 or > 30)
            return BadRequest(new ErrorResponse("Hall size must be 1-20 rows and 1-30 seats per row."));

        var show = new Show
        {
            MovieId = request.MovieId,
            StartsAt = request.StartsAt,
            Price = request.Price,
            Rows = request.Rows,
            SeatsPerRow = request.SeatsPerRow
        };

        db.Shows.Add(show);
        await db.SaveChangesAsync();

        return Ok(ToDto(show));
    }

    private static ShowDto ToDto(Show s) =>
        new(s.Id, s.MovieId, s.StartsAt, s.Price, s.Rows, s.SeatsPerRow);
}
