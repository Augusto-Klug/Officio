using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.ViewModels.Orcamentos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/orcamentos/[action]")]
[Authorize]
public class OrcamentosControlador(IMediator mediator) : ControllerBase
{
    [HttpGet("{prospectoId:guid}")]
    public async Task<IActionResult> PorProspecto(Guid prospectoId, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ObterOrcamentosPorProspectoConsulta(prospectoId),
            cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] CriarOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(), cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(PorProspecto), new { prospectoId = viewModel.ProspectoId }, resultado.Valor)
            : ResultadoErro(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> AlterarStatus(
        Guid id,
        [FromBody] AlterarStatusOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Aceitar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new AceitarOrcamentoComando(id), cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Recusar(
        Guid id,
        [FromBody] RecusarOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpGet]
    public async Task<IActionResult> ListarCatalogo(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarCatalogoOrcamentoConsulta(), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> CriarItemCatalogo(
        [FromBody] CriarItemCatalogoOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(), cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(ListarCatalogo), null, resultado.Valor)
            : ResultadoErro(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarItemCatalogo(
        Guid id,
        [FromBody] AtualizarItemCatalogoOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> AlterarAtivoItemCatalogo(
        Guid id,
        [FromBody] AlterarAtivoItemCatalogoOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("n\u00E3o encontrado", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);
}
