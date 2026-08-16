using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Officio.Infraestrutura.Identidade;

public class ServicoJwt(IConfiguration configuration, ContextoAplicacao db) : IAutenticacaoServico
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public async Task<ResultadoAutenticacao?> AutenticarAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        var resultado = await AuthenticateAsync(email, senha, cancellationToken);
        if (resultado is null)
            return null;

        var (token, expiraEm, usuario) = resultado.Value;
        return new ResultadoAutenticacao(
            token,
            expiraEm,
            new UsuarioAutenticado(usuario.Id, usuario.Nome, usuario.Email, usuario.Role));
    }

    public async Task<(string Token, DateTime ExpiresAt, Usuario Usuario)?> AuthenticateAsync(
        string email,
        string senha,
        CancellationToken ct = default)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null)
            return null;

        var resultadoSenha = _passwordHasher.VerifyHashedPassword(user, user.SenhaHash, senha);
        if (resultadoSenha == PasswordVerificationResult.Failed)
            return null;

        var expiresAt = DateTime.UtcNow.AddHours(8);
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "super-secret-key-Officio-2025!"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("nome", user.Nome),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "Officio",
            audience: configuration["Jwt:Audience"] ?? "Officio",
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user);
    }
}
