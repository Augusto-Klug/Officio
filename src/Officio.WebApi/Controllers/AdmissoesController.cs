using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.CancelarAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirRevisaoDocumental;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarLinkFichaAdmissaoEmail;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AprovarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RemoverDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ValidarDocumentoAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.GerarLinkFichaEmpresaAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RevogarLinkFichaEmpresaAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarDocumentosAdmissao;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarEmpresasComAdmissoes;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterUrlDocumentoAdmissao;
using Officio.Dominio.Contratos;
using Officio.ViewModels.Admissoes;
using Officio.WebApi.Conversores;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admissoes/[action]")]
[Authorize]
public class AdmissoesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? clienteId,
        [FromQuery] string? status,
        [FromQuery] string? busca,
        [FromQuery] Guid? empresaId,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ListarAdmissoesConsulta(clienteId, status, busca, empresaId),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarEmpresasComAdmissoes(
        [FromQuery] string? busca,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ListarEmpresasComAdmissoesConsulta(busca),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterAdmissaoPorIdConsulta(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarAdmissaoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(viewModel.ParaComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Concluir(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ConcluirAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConcluirRevisaoDocumental(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ConcluirRevisaoDocumentalComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarLinkPorEmail(
        Guid id,
        [FromBody] EnviarLinkFichaAdmissaoEmailViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var requisicao = new EnviarLinkFichaAdmissaoEmailRequisicao(
            id, viewModel.Papel, viewModel.Email, viewModel.Token);
        var resultado = await mediator.Send(new EnviarLinkFichaAdmissaoEmailComando(requisicao), cancellationToken);
        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new CancelarAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GerarLinkFicha(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new GerarLinkFichaAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevogarLinkFicha(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new RevogarLinkFichaAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GerarLinkFichaEmpresa(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new GerarLinkFichaEmpresaAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevogarLinkFichaEmpresa(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new RevogarLinkFichaEmpresaAdmissaoComando(id), cancellationToken);
        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarDocumentos(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ListarDocumentosAdmissaoConsulta(id), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}/itens/{itemId:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarDocumento(
        Guid id,
        Guid itemId,
        [FromForm] List<IFormFile> arquivos,
        CancellationToken cancellationToken)
    {
        if (arquivos.Count == 0)
            return BadRequest("Envie ao menos um arquivo.");
        var streams = arquivos.Select(arquivo => arquivo.OpenReadStream()).ToList();
        try
        {
            var uploads = arquivos.Select((arquivo, indice) => new ArquivoAdmissaoUpload(
                streams[indice], arquivo.FileName, arquivo.ContentType, arquivo.Length)).ToList();
            var requisicao = new EnviarDocumentoAdmissaoRequisicao(id, itemId, uploads);
            var resultado = await mediator.Send(new EnviarDocumentoAdmissaoComando(requisicao), cancellationToken);
            return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
        }
        finally
        {
            foreach (var stream in streams)
                await stream.DisposeAsync();
        }
    }

    [HttpGet("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterUrlDocumento(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ObterUrlDocumentoAdmissaoConsulta(id, itemId), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidarDocumento(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new ValidarDocumentoAdmissaoComando(id, itemId), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPost("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprovarDocumento(
        Guid id,
        Guid itemId,
        [FromBody] AprovarDocumentoAdmissaoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var requisicao = new AprovarDocumentoAdmissaoRequisicao(id, itemId, viewModel.Observacao);
        var resultado = await mediator.Send(
            new AprovarDocumentoAdmissaoComando(requisicao), cancellationToken);
        return resultado.Sucesso ? Ok() : ResultadoErro(resultado.Erro);
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverDocumento(
        Guid id,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new RemoverDocumentoAdmissaoComando(id, itemId), cancellationToken);
        return resultado.Sucesso ? NoContent() : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("nao encontrad", StringComparison.OrdinalIgnoreCase) == true ||
           erro?.Contains("não encontrad", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);
}
