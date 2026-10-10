namespace CineBook.Api.Models;

// A person who can sign in. Role is either "User" or "Admin".
public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    // We never store the real password, only the BCrypt hash of it.
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";

    public List<Booking> Bookings { get; set; } = new();
}
