using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ConverterParaCliente;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarSociosProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ListarTiposRelacaoSocietaria;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ExcluirProspecto;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectoPorId;
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
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarSocios(Guid id, CriarProspectoViewModel dados, CancellationToken ct)
    {
        var requisicao = prospectoViewModelConversor.ParaComando(dados).Requisicao;
        var resultado = await mediator.Send(new AtualizarSociosProspectoComando(id, requisicao), ct);
        return resultado.Sucesso ? NoContent() : FalhaAlteracao(resultado.Erro!);
    }
    [HttpGet]
    public async Task<IActionResult> TiposRelacaoSocietaria(CancellationToken ct)
        => Ok(await mediator.Send(new ListarTiposRelacaoSocietariaConsulta(), ct));

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarProspectoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(prospectoViewModelConversor.ParaComando(viewModel), cancellationToken);

        if (resultado.Sucesso)
            return CreatedAtAction(nameof(Listar), new { id = resultado.Valor!.Id }, resultado.Valor);

        return resultado.Erro switch
        {
            LocalidadeErros.CepInvalido or LocalidadeErros.MunicipioDivergenteDoCep
                => UnprocessableEntity(resultado.Erro),
            LocalidadeErros.ConsultaCepIndisponivel
                => StatusCode(StatusCodes.Status503ServiceUnavailable, resultado.Erro),
            _ => BadRequest(resultado.Erro)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProspectoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken, [FromQuery] bool incluirSociosCliente = false)
    {
        var resultado = await mediator.Send(new ObterProspectoPorIdConsulta(id, incluirSociosCliente), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : NotFound(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarProspectoViewModel viewModel, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new AtualizarProspectoComando(id, prospectoViewModelConversor.ParaComando(viewModel).Requisicao), cancellationToken);
        return resultado.Sucesso ? NoContent() : FalhaAlteracao(resultado.Erro!);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ExcluirProspectoComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : FalhaAlteracao(resultado.Erro!);
    }

    private IActionResult FalhaAlteracao(string erro) => erro switch
    {
        ProspectoErros.NaoEncontrado => NotFound(erro),
        ProspectoErros.ForaDaEtapaInicial or ProspectoErros.CadastroBloqueado or ProspectoErros.ContratoAguardandoAssinatura or ProspectoErros.PossuiVinculos or ProspectoErros.ConfirmacaoCancelamentoNecessaria => Conflict(erro),
        LocalidadeErros.CepInvalido or LocalidadeErros.MunicipioDivergenteDoCep => UnprocessableEntity(erro),
        LocalidadeErros.ConsultaCepIndisponivel => StatusCode(StatusCodes.Status503ServiceUnavailable, erro),
        _ => BadRequest(erro)
    };

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
