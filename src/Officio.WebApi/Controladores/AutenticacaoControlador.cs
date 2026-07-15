using Officio.Dominio.Servicos.Interfaces;
using Officio.Infraestrutura.Identidade;
using Microsoft.AspNetCore.Mvc;

namespace Officio.WebApi.Controladores;

[ApiController]
[Controller]
[Route("api/auth")]
public class AutenticacaoControlador(ServicoJwt jwtService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] SolicitacaoLogin request,
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

public record SolicitacaoLogin(string Email, string Senha);






