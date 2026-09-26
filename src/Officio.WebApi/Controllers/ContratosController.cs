using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarEmpresasContratos;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterPdfAssinadoContrato;
using Officio.ViewModels.Contratos;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/contratos/[action]")]
[Authorize]
public class ContratosController(
    IMediator mediator,
    IContratoViewModelConversor contratoViewModelConversor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? empresaId,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarContratosConsulta(empresaId), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet]
    public async Task<IActionResult> ListarEmpresas(CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarEmpresasContratosConsulta(), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterContratoPorIdConsulta(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> GerarPdf(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new GerarPdfContratoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPdfAssinado(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterPdfAssinadoContratoConsulta(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> EnviarParaAssinatura(
        Guid id,
        [FromBody] EnviarAssinaturaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(contratoViewModelConversor.ParaComando(viewModel, id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("n\u00e3o encontrado", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);
}
