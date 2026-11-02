using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;
using Officio.ViewModels.Admissoes;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/portal-cliente/[action]")]
[Authorize(Roles = PerfisAcesso.Cliente)]
public sealed class PortalClienteController(IPortalClienteServico servico) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObterVisao([FromQuery] Guid? empresaId, CancellationToken ct)
    {
        var resultado = await servico.ObterVisaoAsync(empresaId, ct);
        return resultado.Sucesso ? Ok(resultado.Valor) : Forbid();
    }

    [HttpGet("{empresaId:guid}/{admissaoId:guid}")]
    public async Task<IActionResult> ObterAdmissao(Guid empresaId, Guid admissaoId, CancellationToken ct)
    {
        var resultado = await servico.ObterAdmissaoAsync(empresaId, admissaoId, ct);
        return resultado.Sucesso ? Ok(resultado.Valor) : NotFound(resultado.Erro);
    }

    [HttpGet("{empresaId:guid}/{contratoId:guid}")]
    public async Task<IActionResult> ObterPdfAssinado(Guid empresaId, Guid contratoId, CancellationToken ct)
    {
        var resultado = await servico.ObterPdfAssinadoAsync(empresaId, contratoId, ct);
        return resultado.Sucesso ? Ok(resultado.Valor) : NotFound(resultado.Erro);
    }

    [HttpPost]
    public async Task<IActionResult> CriarAdmissao([FromBody] CriarAdmissaoPortalViewModel viewModel, CancellationToken ct)
    {
        var resultado = await servico.CriarAdmissaoAsync(
            new CriarAdmissaoPortalRequisicao(
                viewModel.EmpresaId, viewModel.EmailFuncionario), ct);
        return resultado.Sucesso ? CreatedAtAction(nameof(ObterAdmissao), new
        {
            empresaId = viewModel.EmpresaId,
            admissaoId = resultado.Valor!.Id
        }, resultado.Valor) : BadRequest(resultado.Erro);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> AtualizarDadosEmpregador(
        Guid id,
        [FromBody] AtualizarFichaEmpresaViewModel viewModel,
        CancellationToken ct)
    {
        var resultado = await servico.AtualizarDadosEmpregadorAsync(id, new AtualizarFichaEmpresaRequisicao(
            viewModel.Pis, viewModel.DataAdmissaoPrevista, viewModel.Salario, viewModel.Comissao,
            viewModel.Funcao, viewModel.ContratoExperienciaDias, viewModel.UtilizaValeTransporte,
            viewModel.HorarioTrabalho), ct);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }

    [HttpPost("{id:guid}")]
    public async Task<IActionResult> ConcluirDadosEmpregador(Guid id, CancellationToken ct)
    {
        var resultado = await servico.ConcluirDadosEmpregadorAsync(id, ct);
        return resultado.Sucesso ? NoContent() : BadRequest(resultado.Erro);
    }
}
