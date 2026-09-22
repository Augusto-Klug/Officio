using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Comandos.Atualizar;
using Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Consultas.ObterPorProspecto;
using Officio.ViewModels.FormulariosAbertura;

namespace Officio.WebApi.Controllers;

[ApiController]
[Route("api/v1/formularios-abertura")]
[Authorize]
public class FormulariosAberturaController(IMediator mediator) : ControllerBase
{
    [HttpGet("prospectos/{prospectoId:guid}")]
    public async Task<IActionResult> ObterPorProspecto(Guid prospectoId, CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(new ObterFormularioAberturaPorProspectoConsulta(prospectoId), cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    [HttpPut("prospectos/{prospectoId:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid prospectoId,
        [FromBody] AtualizarFormularioAberturaViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var resultado = await mediator.Send(
            new AtualizarFormularioAberturaComando(ParaRequisicao(prospectoId, viewModel)),
            cancellationToken);
        return resultado.Sucesso ? Ok(resultado.Valor) : ResultadoErro(resultado.Erro);
    }

    private IActionResult ResultadoErro(string? erro)
        => erro?.Contains("nao encontrad", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(erro)
            : BadRequest(erro);

    private static AtualizarFormularioAberturaRequisicao ParaRequisicao(
        Guid prospectoId,
        AtualizarFormularioAberturaViewModel viewModel)
        => new(
            prospectoId, viewModel.PessoaResponsavelId, viewModel.EnderecoId, viewModel.NomeEmpresaPretendido,
            viewModel.NomeFantasiaPretendido, viewModel.CadastroIptu, viewModel.NomeEdificacao,
            viewModel.ProprietarioImovelNome, viewModel.ProprietarioImovelCpfCnpj, viewModel.AreaTotalEdificacaoM2,
            viewModel.AreaOcupadaEmpresaM2, viewModel.CapitalSocialPrevisto, viewModel.EmailEmpresaPretendido,
            viewModel.TelefoneEmpresaPretendido, viewModel.NumeroFuncionariosPrevisto, viewModel.NumeroProLaboresPrevisto,
            viewModel.LotacaoMaximaPublico, viewModel.HorarioFuncionamento, viewModel.DiasTrabalhadosMes,
            viewModel.SituacaoImovel, viewModel.NumeroMaquinas, viewModel.TipoMaquinas,
            viewModel.EnquadramentoPretendido, viewModel.Observacoes, viewModel.TipoEmpresaPretendido,
            viewModel.RegimeTributarioPretendido,
            (viewModel.Atividades ?? []).Select(item => new AtividadeFormularioAberturaRequisicao(
                item.DescricaoAtividade, item.Cnae, item.Percentual, item.RealizaNoEndereco, item.Ordem)).ToList());
}
