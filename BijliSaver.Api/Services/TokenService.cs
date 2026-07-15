// Creates signed JWTs. The signing key lives in appsettings ("Jwt:Key")
// and must be long & secret — never committed with a real value.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BijliSaver.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace BijliSaver.Api.Services;

public class TokenService(IConfiguration config)
{
    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Issuer"],
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
            ],
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
