using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ListarClientes;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ObterClientePorId;
using Officio.ViewModels.Clientes;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/clientes/[action]")]
[Authorize]
public class ClientesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] bool? ativo,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarClientesConsulta(busca, ativo), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterClientePorIdConsulta(id), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarClienteViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TentarConverter(() => viewModel.ParaComando(id), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> VincularCodigoSci(
        Guid id,
        [FromBody] VincularCodigoSciViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("não encontrado", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);

    private static bool TentarConverter<TComando>(
        Func<TComando> conversor,
        out TComando? comando,
        out IActionResult? erro)
    {
        try
        {
            comando = conversor();
            erro = null;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            comando = default;
            erro = new BadRequestObjectResult("Regime tributario invalido.");
            return false;
        }
    }
}
