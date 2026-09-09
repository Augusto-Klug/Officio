using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ContratoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IContratoRepositorio
{
    public Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default)
        => AdicionarAsync<Contrato>(contrato, cancellationToken);

    public Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .FirstOrDefaultAsync(contrato => contrato.BudgetId == orcamentoId, cancellationToken);

    public Task<List<Contrato>> ListarAsync(CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .OrderByDescending(contrato => contrato.CriadoEm)
            .ToListAsync(cancellationToken);

    public Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .Include(contrato => contrato.Orcamento)
                .ThenInclude(orcamento => orcamento!.Itens)
            .FirstOrDefaultAsync(contrato => contrato.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Prospecto)
                .ThenInclude(p => p.Socios)
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl!.Socios)
            .Include(c => c.Orcamento)
                .ThenInclude(o => o!.Itens)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public Task<AssinaturaSolicitacao?> ObterSolicitacaoAssinaturaPorIdExternoAsync(
        string provedor,
        string idExterno,
        CancellationToken cancellationToken = default)
        => Contexto.AssinaturaSolicitacoes
            .IgnoreQueryFilters()
            .Include(solicitacao => solicitacao.Contrato)
            .Include(solicitacao => solicitacao.Signatarios)
            .Include(solicitacao => solicitacao.Eventos)
            .FirstOrDefaultAsync(
                solicitacao => solicitacao.Provedor == provedor && solicitacao.IdExterno == idExterno,
                cancellationToken);

    public async Task<StatusAssinatura> ObterOuCriarStatusAssinaturaAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default)
    {
        var statusLocal = Contexto.StatusAssinaturas.Local
            .FirstOrDefault(item => item.Codigo == codigo);
        if (statusLocal is not null)
            return statusLocal;

        var status = await Contexto.StatusAssinaturas
            .FirstOrDefaultAsync(item => item.Codigo == codigo, cancellationToken);

        if (status is not null)
            return status;

        status = new StatusAssinatura(codigo, descricao, ordem);
        await Contexto.StatusAssinaturas.AddAsync(status, cancellationToken);
        return status;
    }

    public Task AdicionarSolicitacaoAssinaturaAsync(
        AssinaturaSolicitacao solicitacao,
        CancellationToken cancellationToken = default)
        => AdicionarAsync<AssinaturaSolicitacao>(solicitacao, cancellationToken);

    public Task<bool> ExisteEventoAssinaturaAsync(
        Guid assinaturaSolicitacaoId,
        string tipoEvento,
        string? payload,
        CancellationToken cancellationToken = default)
        => Contexto.AssinaturaEventos.AnyAsync(
            evento => evento.AssinaturaSolicitacaoId == assinaturaSolicitacaoId
                      && evento.TipoEvento == tipoEvento
                      && evento.Payload == payload,
            cancellationToken);

    public Task AdicionarEventoAssinaturaAsync(
        AssinaturaEvento evento,
        CancellationToken cancellationToken = default)
        => AdicionarAsync<AssinaturaEvento>(evento, cancellationToken);

    public Task<bool> ExisteDocumentoAssinadoAsync(
        Guid contratoId,
        CancellationToken cancellationToken = default)
        => Contexto.Documents.AnyAsync(
            documento => documento.ContratoId == contratoId
                         && documento.Origem == "ASSINATURA_AUTENTIQUE",
            cancellationToken);

    public Task<Documento?> ObterDocumentoAssinadoAsync(
        Guid contratoId,
        CancellationToken cancellationToken = default)
        => Contexto.Documents
            .Where(documento => documento.ContratoId == contratoId
                                && documento.Origem == "ASSINATURA_AUTENTIQUE")
            .OrderByDescending(documento => documento.CriadoEm)
            .FirstOrDefaultAsync(cancellationToken);

    public Task AdicionarDocumentoAsync(
        Documento documento,
        CancellationToken cancellationToken = default)
        => AdicionarAsync<Documento>(documento, cancellationToken);

    public async Task AdicionarAssinaturaAsync(Assinatura assinatura, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Assinatura>(assinatura, cancellationToken);
}
