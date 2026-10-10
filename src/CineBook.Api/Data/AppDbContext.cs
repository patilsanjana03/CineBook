using CineBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CineBook.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Two accounts can't share an email.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Show>()
            .Property(s => s.Price)
            .HasPrecision(10, 2);

        // This is the real protection against double booking. Even if two people click
        // "confirm" at the same instant, the database refuses the second insert.
        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.ShowId, b.SeatRow, b.SeatNumber })
            .IsUnique();
    }
}
