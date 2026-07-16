using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Interfaces;
using Officio.ViewModels.Autenticacao;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/autenticacao/[action]")]
public class AutenticacaoControlador(IAutenticacaoServico autenticacaoServico) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await autenticacaoServico.AutenticarAsync(
            viewModel.Email,
            viewModel.Senha,
            cancellationToken);

        return resultado is null
            ? Unauthorized(new { error = "E-mail ou senha incorretos." })
            : Ok(resultado);
    }
}
