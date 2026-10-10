using CineBook.Api.Models;

namespace CineBook.Api.Data;

// Fills an empty database with demo data so the app is usable straight after the first run.
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any() || db.Movies.Any())
        {
            return;
        }

        db.Users.AddRange(
            new User
            {
                FullName = "Admin",
                Email = "admin@cinebook.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin"
            },
            new User
            {
                FullName = "Demo User",
                Email = "user@cinebook.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = "User"
            });

        var movies = new List<Movie>
        {
            new() { Title = "Midnight Express Lane", Genre = "Thriller", DurationMinutes = 128,
                    Description = "A night-shift train driver stumbles onto a smuggling ring and has until sunrise to get the evidence to the police." },
            new() { Title = "The Last Monsoon", Genre = "Drama", DurationMinutes = 142,
                    Description = "Three generations of a coastal family fight to save their home as the final big storm of the season approaches." },
            new() { Title = "Pixel Heist", Genre = "Comedy", DurationMinutes = 104,
                    Description = "A group of unemployed game testers plans the world's most awkward bank robbery, using only what they learned from video games." },
            new() { Title = "Orbit 9", Genre = "Sci-Fi", DurationMinutes = 118,
                    Description = "The crew of a failing space station has nine orbits to repair the engines before the planet below pulls them in." }
        };

        // Two shows a day (6 PM and 9 PM) for the next three days.
        foreach (var movie in movies)
        {
            for (var day = 0; day < 3; day++)
            {
                var date = DateTime.Today.AddDays(day);
                movie.Shows.Add(new Show { StartsAt = date.AddHours(18), Price = 200, Rows = 6, SeatsPerRow = 8 });
                movie.Shows.Add(new Show { StartsAt = date.AddHours(21), Price = 250, Rows = 6, SeatsPerRow = 8 });
            }
        }

        db.Movies.AddRange(movies);
        db.SaveChanges();
    }
}
