using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.ViewModels.Contratos;
using Officio.ViewModels.Orcamentos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/orcamentos/[action]")]
[Authorize]
public class OrcamentosController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterOrcamentoPorIdConsulta(id), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

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
        if (!TentarConverter(() => viewModel.ParaComando(), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

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
        if (!TentarConverter(() => viewModel.ParaComando(id), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> AlterarStatus(
        Guid id,
        [FromBody] AlterarStatusOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TentarConverter(() => viewModel.ParaComando(id), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Aceitar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new AceitarOrcamentoComando(id), cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> ConverterParaContrato(
        Guid id,
        [FromBody] ConverterOrcamentoParaContratoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction("ObterPorId", "Contratos", new { id = resultado.Valor }, resultado.Valor)
            : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Recusar(
        Guid id,
        [FromBody] RecusarOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TentarConverter(() => viewModel.ParaComando(id), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpGet]
    public async Task<IActionResult> Catalogo(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarCatalogoOrcamentoConsulta(), cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> CriarItemCatalogo(
        [FromBody] CriarItemCatalogoOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TentarConverter(() => viewModel.ParaComando(), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso
            ? CreatedAtAction(nameof(Catalogo), null, resultado.Valor)
            : ResultadoErro(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarItemCatalogo(
        Guid id,
        [FromBody] AtualizarItemCatalogoOrcamentoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (!TentarConverter(() => viewModel.ParaComando(id), out var comando, out var erro))
            return erro!;

        var resultado = await mediator.Send(comando!, cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPatch("{id:guid}")]
    public Task<IActionResult> AtivarItemCatalogo(Guid id, CancellationToken cancellationToken)
        => AlterarAtivoItemCatalogo(id, true, cancellationToken);

    [HttpPatch("{id:guid}")]
    public Task<IActionResult> DesativarItemCatalogo(Guid id, CancellationToken cancellationToken)
        => AlterarAtivoItemCatalogo(id, false, cancellationToken);

    private async Task<IActionResult> AlterarAtivoItemCatalogo(
        Guid id,
        bool ativo,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new AlterarAtivoItemCatalogoOrcamentoComando(id, ativo),
            cancellationToken);

        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("n\u00E3o encontrado", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase) == true
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
        catch (ArgumentOutOfRangeException excecao)
        {
            comando = default;
            erro = new BadRequestObjectResult(excecao.ParamName switch
            {
                "secao" => "Secao do item de orcamento invalida.",
                "tipo" => "Tipo do item de orcamento invalido.",
                "status" => "Status do orcamento invalido.",
                _ => "Valor de enumerador invalido."
            });
            return false;
        }
    }
}
