using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarComoLida;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarTodasComoLidas;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Consultas.ListarNotificacoes;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/notificacoes/[action]")]
[Authorize]
public class NotificacoesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Listar(
        [FromQuery] bool? lida,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ListarNotificacoesConsulta(lida),
            cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarComoLida(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new MarcarNotificacaoComoLidaComando(id),
            cancellationToken);

        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    [HttpPatch]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> MarcarTodasComoLidas(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new MarcarTodasNotificacoesComoLidasComando(),
            cancellationToken);

        return resultado.Sucesso
            ? Ok(new { quantidade = resultado.Valor })
            : BadRequest(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("nao encontrada", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);
}
