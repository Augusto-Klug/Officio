using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContaFacil.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(JwtService jwtService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        // TODO: hash da senha antes de comparar
        var result = await jwtService.AuthenticateAsync(request.Email, request.Senha, ct);

        if (result is null)
            return Unauthorized(new { error = "E-mail ou senha incorretos." });

        var (token, expiresAt, user) = result.Value;

        return Ok(new
        {
            token,
            expiresAt,
            user = new { user.Id, user.Nome, user.Email, user.Role }
        });
    }
}

public record LoginRequest(string Email, string Senha);
