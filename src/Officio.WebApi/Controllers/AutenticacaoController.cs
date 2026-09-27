using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.AceitarConvite;
using Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.RedefinirSenha;
using Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.SolicitarRedefinicaoSenha;
using Officio.ViewModels.Autenticacao;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/autenticacao/[action]")]
[AllowAnonymous]
public class AutenticacaoController(IAutenticacaoServico autenticacaoServico, IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AceitarConvite(
        [FromBody] AceitarConviteViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new AceitarConviteComando(viewModel.Token, viewModel.Senha), cancellationToken);
        return resultado.Sucesso ? Ok() : BadRequest(new { error = resultado.Erro });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> EsqueciSenha(
        [FromBody] EsqueciSenhaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new SolicitarRedefinicaoSenhaComando(viewModel.Email), cancellationToken);
        return Accepted(new { message = "Se o e-mail estiver cadastrado, as instrucoes serao enviadas." });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedefinirSenha(
        [FromBody] RedefinirSenhaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new RedefinirSenhaComando(viewModel.Token, viewModel.Senha), cancellationToken);
        return resultado.Sucesso ? Ok() : BadRequest(new { error = resultado.Erro });
    }
}
