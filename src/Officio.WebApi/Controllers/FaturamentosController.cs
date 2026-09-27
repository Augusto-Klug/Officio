using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Cancelar;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Fechar;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.FecharCompetencia;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.GerarCompetencia;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.RecalcularCompetencia;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Listar;
using Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Resumo;
using Officio.ViewModels.Faturamentos;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/faturamentos/[action]")]
[Authorize]
public class FaturamentosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? competencia,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarFaturamentosConsulta(competencia, status), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GerarCompetencia(
        [FromBody] GerarFaturamentoCompetenciaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new GerarFaturamentoCompetenciaComando(viewModel.Competencia),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RecalcularCompetencia(
        [FromBody] GerarFaturamentoCompetenciaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new RecalcularFaturamentoCompetenciaComando(viewModel.Competencia),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Resumo(
        [FromQuery] DateOnly competencia,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ObterResumoFaturamentoConsulta(competencia),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> FecharCompetencia(
        [FromBody] GerarFaturamentoCompetenciaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new FecharFaturamentoCompetenciaComando(viewModel.Competencia),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Fechar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new FecharFaturamentoComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new CancelarFaturamentoComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }
}
