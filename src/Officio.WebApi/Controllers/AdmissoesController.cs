using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;
using Officio.ViewModels.Admissoes;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admissoes/[action]")]
[Authorize]
public class AdmissoesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? clienteId,
        [FromQuery] string? status,
        [FromQuery] string? busca,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ListarAdmissoesConsulta(clienteId, status, busca),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterAdmissaoPorIdConsulta(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> CriarManual(
        [FromBody] CriarAdmissaoManualViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(), cancellationToken);
        return resultado.Sucesso
            ? CreatedAtAction(nameof(ObterPorId), new { id = resultado.Valor }, resultado.Valor)
            : ResultadoErro(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarAdmissaoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Concluir(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ConcluirAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new CancelarAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("nao encontrad", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("não encontrad", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);
}
