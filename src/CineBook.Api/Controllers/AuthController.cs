using CineBook.Api.Data;
using CineBook.Api.Dtos;
using CineBook.Api.Models;
using CineBook.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineBook.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new ErrorResponse("Please enter your name."));
        if (!email.Contains('@'))
            return BadRequest(new ErrorResponse("Please enter a valid email address."));
        if (request.Password.Length < 6)
            return BadRequest(new ErrorResponse("Password must be at least 6 characters."));

        if (await db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new ErrorResponse("An account with this email already exists."));

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "User" // Nobody can make themselves an admin through the public endpoint.
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Same message for "no such user" and "wrong password" so attackers can't
        // use the error to find out which emails are registered.
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new ErrorResponse("Wrong email or password."));

        return Ok(ToResponse(user));
    }

    private AuthResponse ToResponse(User user) =>
        new(tokens.CreateToken(user), user.FullName, user.Email, user.Role);
}
