using CineBook.Api.Data;
using CineBook.Api.Dtos;
using CineBook.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBook.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController(AppDbContext db) : ControllerBase
{
    // Anyone can browse movies, even without signing in.
    [HttpGet]
    public async Task<ActionResult<List<MovieDto>>> GetAll()
    {
        var movies = await db.Movies
            .AsNoTracking() // read-only query, so EF doesn't need to track changes
            .OrderBy(m => m.Title)
            .ToListAsync();

        return Ok(movies.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var movie = await db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        return movie is null ? NotFound(new ErrorResponse("Movie not found.")) : Ok(ToDto(movie));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieDto>> Create(CreateMovieRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new ErrorResponse("Title is required."));
        if (request.DurationMinutes <= 0)
            return BadRequest(new ErrorResponse("Duration must be more than zero."));

        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Genre = request.Genre.Trim(),
            DurationMinutes = request.DurationMinutes
        };

        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, ToDto(movie));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie is null)
            return NotFound(new ErrorResponse("Movie not found."));

        db.Movies.Remove(movie); // its shows and bookings are removed too (cascade delete)
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static MovieDto ToDto(Movie m) =>
        new(m.Id, m.Title, m.Description, m.Genre, m.DurationMinutes);
}
