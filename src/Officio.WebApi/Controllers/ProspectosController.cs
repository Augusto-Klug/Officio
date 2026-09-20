using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;
using Officio.ViewModels.Prospectos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/prospectos/[action]")]
[Authorize]
public class ProspectosController(
    IMediator mediator,
    IProspectoViewModelConversor prospectoViewModelConversor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? busca = null,
        [FromQuery] Guid? responsavelId = null,
        CancellationToken cancellationToken = default)
    {
        var resultado = await mediator.Send(
            prospectoViewModelConversor.ParaConsulta(pagina, tamanhoPagina, status, busca, responsavelId),
            cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] CriarProspectoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(prospectoViewModelConversor.ParaComando(viewModel), cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(Listar), new { id = resultado.Valor!.Id }, resultado.Valor)
            : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> ConverterParaCliente(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ConverterProspectoComando(id), cancellationToken);

        if (!resultado.Sucesso)
            return resultado.Erro!.Contains("encontrado", StringComparison.OrdinalIgnoreCase)
                ? NotFound(resultado.Erro)
                : BadRequest(resultado.Erro);

        return CreatedAtAction(nameof(ConverterParaCliente), new
        {
            clienteId = resultado.Valor,
            mensagem = "Prospecto convertido com sucesso."
        });
    }
}
