using ContaFacil.Domain.Entities;
using ContaFacil.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContaFacil.Infrastructure.Identity;

public class JwtService(IConfiguration configuration, AppDbContext db)
{
    public async Task<(string Token, DateTime ExpiresAt, User User)?> AuthenticateAsync(
        string email, string senhaHash, CancellationToken ct = default)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email && u.SenhaHash == senhaHash, ct);

        if (user is null)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-key-contafacil-2025!"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("nome", user.Nome),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "ContaFacil",
            audience: configuration["Jwt:Audience"] ?? "ContaFacil",
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user);
    }
}
