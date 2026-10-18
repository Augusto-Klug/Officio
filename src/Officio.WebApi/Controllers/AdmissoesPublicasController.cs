using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.ViewModels.Admissoes;

namespace Officio.WebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/admissoes-publicas/[action]")]
public class AdmissoesPublicasController(IFichaAdmissaoServico servico) : ControllerBase
{
    [HttpGet("{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Obter(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ObterAsync(token, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPut("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        string token,
        [FromBody] AtualizarFichaFuncionarioViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var requisicao = new AtualizarFichaFuncionarioRequisicao(
            viewModel.NomeFuncionario, viewModel.DataNascimento, viewModel.Sexo,
            viewModel.EstadoCivil, viewModel.Escolaridade, viewModel.Telefone, viewModel.Email,
            (viewModel.Dependentes ?? []).Select(item => new DependenteAdmissaoRequisicao(
                item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
                item.DependenteIr, item.SalarioFamilia)).ToList());
        var resultado = await servico.AtualizarAsync(token, requisicao, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpGet("{token}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterEmpresa(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ObterEmpresaAsync(token, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPut("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarEmpresa(
        string token,
        [FromBody] AtualizarFichaEmpresaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var requisicao = new AtualizarFichaEmpresaRequisicao(
            viewModel.Pis, viewModel.DataAdmissaoPrevista, viewModel.Salario, viewModel.Comissao,
            viewModel.Funcao, viewModel.ContratoExperienciaDias, viewModel.UtilizaValeTransporte,
            viewModel.HorarioTrabalho, viewModel.Banco, viewModel.Agencia, viewModel.Conta, viewModel.TipoConta);
        var resultado = await servico.AtualizarEmpresaAsync(token, requisicao, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConcluirEmpresa(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ConcluirEmpresaAsync(token, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Concluir(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ConcluirAsync(token, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{token}/itens/{itemId:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarDocumento(
        string token,
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
            var resultado = await servico.EnviarDocumentoAsync(token, itemId, uploads, cancellationToken);
            return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
        }
        finally
        {
            foreach (var stream in streams)
                await stream.DisposeAsync();
        }
    }

    [HttpGet("{token}/itens/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterUrlDocumento(
        string token,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var resultado = await servico.ObterUrlDocumentoAsync(token, itemId, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }
}
