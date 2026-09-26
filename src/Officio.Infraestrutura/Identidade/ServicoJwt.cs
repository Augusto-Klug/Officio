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
        email = email.Trim();
        var user = await db.Users
            // O login e publico e ocorre antes de existir contexto de tenant; o email unico evita ambiguidade entre empresas.
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !user.Ativo)
            return null;

        if (!string.Equals(user.Role, "PlatformAdmin", StringComparison.OrdinalIgnoreCase))
        {
            var parceiro = await db.Parceiros.FirstOrDefaultAsync(item => item.Id == user.TenantId, ct);
            if (parceiro is null || !parceiro.Ativo)
                return null;
        }

        var resultadoSenha = _passwordHasher.VerifyHashedPassword(user, user.SenhaHash, senha);
        if (resultadoSenha == PasswordVerificationResult.Failed)
            return null;

        var expirationHours = configuration.GetValue<double?>("Jwt:ExpirationHours") ?? 8;
        if (!double.IsFinite(expirationHours) || expirationHours <= 0)
            throw new InvalidOperationException("A configuracao 'Jwt:ExpirationHours' deve ser um numero positivo.");
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new InvalidOperationException("A configuracao 'Jwt:Key' e obrigatoria para autenticacao JWT.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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
