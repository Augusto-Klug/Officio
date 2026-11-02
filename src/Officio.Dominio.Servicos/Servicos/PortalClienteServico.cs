using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class PortalClienteServico(
    IGestaoAcessoRepositorio acessoRepositorio,
    IAdmissaoRepositorio admissaoRepositorio,
    IAdmissaoServico admissaoServico,
    IContratoServico contratoServico,
    IFichaAdmissaoServico fichaServico,
    IEmpresaAtual empresaAtual,
    IUsuarioAtual usuarioAtual,
    ILogger<PortalClienteServico> logger) : IPortalClienteServico
{
    public async Task<Resultado<VisaoPortalClienteDto>> ObterVisaoAsync(Guid? empresaId, CancellationToken ct = default)
    {
        var empresas = await acessoRepositorio.ListarEmpresasUsuarioAsync(usuarioAtual.UserId, empresaAtual.TenantId, ct);
        if (empresaId.HasValue && empresas.All(empresa => empresa.Id != empresaId.Value))
            return Resultado<VisaoPortalClienteDto>.CriarFalha("Empresa nao vinculada ao usuario.");

        var selecionadas = empresaId.HasValue
            ? empresas.Where(empresa => empresa.Id == empresaId.Value).ToList()
            : empresas;
        var admissoes = new List<AdmissaoResumoDto>();
        var contratos = new List<ContratoDto>();
        foreach (var empresa in selecionadas)
        {
            var resultadoAdmissoes = await admissaoServico.ListarAsync(null, null, null, empresa.Id, ct);
            if (resultadoAdmissoes.Sucesso)
                admissoes.AddRange(resultadoAdmissoes.Valor!);
            var resultadoContratos = await contratoServico.ListarAsync(empresa.Id, null, ct);
            if (resultadoContratos.Sucesso)
                contratos.AddRange(resultadoContratos.Valor!);
        }

        return Resultado<VisaoPortalClienteDto>.CriarSucesso(new(
            empresas.Select(empresa => new EmpresaPortalClienteDto(empresa.Id, empresa.RazaoSocial, empresa.Cnpj)).ToList(),
            admissoes.OrderByDescending(item => item.CriadoEm).ToList(),
            contratos.OrderByDescending(item => item.DataInicio).ToList()));
    }

    public async Task<Resultado<AdmissaoDetalheDto>> ObterAdmissaoAsync(
        Guid empresaId,
        Guid admissaoId,
        CancellationToken ct = default)
    {
        if (!await acessoRepositorio.UsuarioTemAcessoEmpresaAsync(usuarioAtual.UserId, empresaId, empresaAtual.TenantId, ct))
            return Resultado<AdmissaoDetalheDto>.CriarFalha("Empresa nao vinculada ao usuario.");
        var admissao = await admissaoRepositorio.ObterDetalheAsync(admissaoId, ct);
        if (admissao is null || admissao.EmpresaId != empresaId)
            return Resultado<AdmissaoDetalheDto>.CriarFalha("Admissao nao encontrada.");
        return await admissaoServico.ObterPorIdAsync(admissaoId, ct);
    }

    public async Task<Resultado<ResultadoPdfAssinadoContratoDto>> ObterPdfAssinadoAsync(
        Guid empresaId,
        Guid contratoId,
        CancellationToken ct = default)
    {
        if (!await acessoRepositorio.UsuarioTemAcessoEmpresaAsync(usuarioAtual.UserId, empresaId, empresaAtual.TenantId, ct))
            return Resultado<ResultadoPdfAssinadoContratoDto>.CriarFalha("Empresa nao vinculada ao usuario.");

        var contratosDaEmpresa = await contratoServico.ListarAsync(empresaId, null, ct);
        if (!contratosDaEmpresa.Sucesso || contratosDaEmpresa.Valor!.All(item => item.Id != contratoId))
            return Resultado<ResultadoPdfAssinadoContratoDto>.CriarFalha("Contrato nao encontrado para esta empresa.");

        return await contratoServico.ObterPdfAssinadoAsync(contratoId, ct);
    }

    public async Task<Resultado<AdmissaoPortalCriadaDto>> CriarAdmissaoAsync(
        CriarAdmissaoPortalRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (!await acessoRepositorio.UsuarioTemAcessoEmpresaAsync(
                usuarioAtual.UserId, requisicao.EmpresaId, empresaAtual.TenantId, ct))
            return Resultado<AdmissaoPortalCriadaDto>.CriarFalha("Empresa nao vinculada ao usuario.");
        try { _ = new MailAddress(requisicao.EmailFuncionario); }
        catch (FormatException) { return Resultado<AdmissaoPortalCriadaDto>.CriarFalha("Informe um e-mail valido para o funcionario."); }

        var empresa = await acessoRepositorio.EmpresaPertenceAoParceiroAsync(
            requisicao.EmpresaId, empresaAtual.TenantId, ct);
        if (!empresa)
            return Resultado<AdmissaoPortalCriadaDto>.CriarFalha("Empresa inativa ou nao encontrada.");

        var status = await admissaoRepositorio.ObterOuCriarStatusAsync(
            StatusAdmissaoCodigos.Rascunho, "Rascunho", 1, ct);
        const string nomePendente = "Aguardando preenchimento";
        var admissao = Admissao.Criar(empresaAtual.TenantId, requisicao.EmpresaId, status.Id, nomePendente);
        admissao.AtualizarDados(
            nomePendente, null, null, null, null, null, null,
            null, requisicao.EmailFuncionario, null, null, null, null, null,
            null, null, null, null, null, null, null, null);
        var configuracoes = await admissaoRepositorio.ObterOuCriarConfiguracoesDocumentoPadraoAsync(ct);
        admissao.AdicionarDocumentos(configuracoes.Select(configuracao => AdmissaoDocumento.Criar(
            admissao.Id, configuracao.TipoDocumentoId, configuracao.Obrigatorio,
            configuracao.Ordem, configuracao.RotuloExibicao)));
        await admissaoRepositorio.AdicionarAsync(admissao, ct);
        await admissaoRepositorio.SalvarAlteracoesAsync(ct);

        var linkEnviado = false;
        try
        {
            var link = await fichaServico.GerarLinkAsync(admissao.Id, ct);
            if (link.Sucesso)
            {
                var envio = await fichaServico.EnviarLinkPorEmailAsync(new(
                    admissao.Id, "funcionario", requisicao.EmailFuncionario, link.Valor!.Token), ct);
                linkEnviado = envio.Sucesso;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admissao criada, mas nao foi possivel enviar o link ao funcionario. AdmissaoId={AdmissaoId}", admissao.Id);
        }

        var resultado = await admissaoServico.ObterPorIdAsync(admissao.Id, ct);
        return resultado.Sucesso
            ? Resultado<AdmissaoPortalCriadaDto>.CriarSucesso(new(admissao.Id, requisicao.EmailFuncionario, linkEnviado))
            : Resultado<AdmissaoPortalCriadaDto>.CriarFalha(resultado.Erro!);
    }

    public async Task<Resultado<bool>> AtualizarDadosEmpregadorAsync(
        Guid admissaoId,
        AtualizarFichaEmpresaRequisicao requisicao,
        CancellationToken ct = default)
    {
        var admissao = await admissaoRepositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null || !await acessoRepositorio.UsuarioTemAcessoEmpresaAsync(
                usuarioAtual.UserId, admissao.EmpresaId, empresaAtual.TenantId, ct))
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        return await fichaServico.AtualizarEmpresaPortalAsync(admissaoId, requisicao, ct);
    }

    public async Task<Resultado<bool>> ConcluirDadosEmpregadorAsync(Guid admissaoId, CancellationToken ct = default)
    {
        var admissao = await admissaoRepositorio.ObterParaAlteracaoAsync(admissaoId, ct);
        if (admissao is null || !await acessoRepositorio.UsuarioTemAcessoEmpresaAsync(
                usuarioAtual.UserId, admissao.EmpresaId, empresaAtual.TenantId, ct))
            return Resultado<bool>.CriarFalha("Admissao nao encontrada.");
        return await fichaServico.ConcluirEmpresaPortalAsync(admissaoId, ct);
    }
}
