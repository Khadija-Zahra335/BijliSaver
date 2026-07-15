// POST /api/auth/register — create account
// POST /api/auth/login    — get a token
// Password hashing: ASP.NET Core's built-in PasswordHasher (PBKDF2),
// no extra packages, industry-standard, easy to explain in interviews.

using BijliSaver.Api.Data;
using BijliSaver.Api.Dtos;
using BijliSaver.Api.Models;
using BijliSaver.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BijliSaver.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    private static readonly PasswordHasher<User> Hasher = new();

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(email))
            return BadRequest("Name and email are required.");
        if (req.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters.");
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            return Conflict("An account with this email already exists. Try signing in.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = req.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = Hasher.HashPassword(user, req.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return new AuthResponse(tokens.CreateToken(user), user.DisplayName!, user.Email!);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

        // Same message for wrong email and wrong password — never reveal which.
        if (user?.PasswordHash is null ||
            Hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password)
                == PasswordVerificationResult.Failed)
            return Unauthorized("Email or password is incorrect.");

        return new AuthResponse(tokens.CreateToken(user), user.DisplayName ?? "", user.Email!);
    }
}
