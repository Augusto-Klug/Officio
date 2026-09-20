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
    public async Task<IActionResult> Obter(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ObterAsync(token, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPut("{token}")]
    public async Task<IActionResult> Atualizar(
        string token,
        [FromBody] AtualizarFichaAdmissaoViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var requisicao = new AtualizarFichaAdmissaoRequisicao(
            viewModel.NomeFuncionario, viewModel.Cpf, viewModel.Rg, viewModel.DataNascimento,
            viewModel.Sexo, viewModel.EstadoCivil, viewModel.Escolaridade, viewModel.Telefone,
            viewModel.Email, viewModel.Pis, viewModel.Banco, viewModel.Agencia, viewModel.Conta,
            viewModel.TipoConta, viewModel.UtilizaValeTransporte,
            (viewModel.Dependentes ?? []).Select(item => new DependenteAdmissaoRequisicao(
                item.Nome, item.Cpf, item.DataNascimento, item.Parentesco,
                item.DependenteIr, item.SalarioFamilia)).ToList());
        var resultado = await servico.AtualizarAsync(token, requisicao, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{token}")]
    public async Task<IActionResult> Concluir(string token, CancellationToken cancellationToken)
    {
        var resultado = await servico.ConcluirAsync(token, cancellationToken);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{token}/itens/{itemId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> EnviarDocumento(
        string token,
        Guid itemId,
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo.Length == 0)
            return BadRequest("Arquivo nao informado.");
        await using var stream = arquivo.OpenReadStream();
        var resultado = await servico.EnviarDocumentoAsync(
            token, itemId, stream, arquivo.FileName, arquivo.ContentType, arquivo.Length, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpGet("{token}/itens/{itemId:guid}")]
    public async Task<IActionResult> ObterUrlDocumento(
        string token,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var resultado = await servico.ObterUrlDocumentoAsync(token, itemId, cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : BadRequest(resultado.Erro);
    }
}
