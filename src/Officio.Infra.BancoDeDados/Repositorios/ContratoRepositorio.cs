using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ContratoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IContratoRepositorio
{
    public Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default)
        => AdicionarAsync<Contrato>(contrato, cancellationToken);

    public Task<Guid?> ObterProspectoIdAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts.Where(c => c.Id == contratoId).Select(c => (Guid?)c.ProspectId).FirstOrDefaultAsync(cancellationToken);

    public Task<Contrato?> ObterPorOrcamentoAsync(Guid orcamentoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .FirstOrDefaultAsync(contrato => contrato.BudgetId == orcamentoId && contrato.Status != StatusContrato.Cancelado, cancellationToken);

    public Task<List<Contrato>> ListarAsync(
        Guid? empresaId = null,
        StatusContrato? status = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.Contracts.AsQueryable();

        if (empresaId.HasValue)
            consulta = consulta.Where(contrato => contrato.EmpresaId == empresaId.Value);

        if (status.HasValue)
            consulta = consulta.Where(contrato => contrato.Status == status.Value);

        return consulta
            .AsNoTracking()
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .Include(contrato => contrato.Orcamento)
                .ThenInclude(orcamento => orcamento!.Itens)
            .Include(contrato => contrato.SolicitacoesAssinatura
                .OrderByDescending(solicitacao => solicitacao.SolicitadoEm)
                .Take(1))
                .ThenInclude(solicitacao => solicitacao.Signatarios)
            .OrderByDescending(contrato => contrato.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Contrato>> ListarAtivosPorEmpresaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Where(contrato => contrato.EmpresaId == empresaId && contrato.Status == StatusContrato.Ativo)
            .OrderByDescending(contrato => contrato.AssinadoEm)
            .ToListAsync(cancellationToken);

    public Task<List<Contrato>> ListarContratosExpiradosAsync(
        DateTime antesDe,
        CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .IgnoreQueryFilters()
            .Where(contrato => contrato.Status == StatusContrato.Ativo
                && contrato.DataFim.HasValue
                && contrato.DataFim.Value < antesDe)
            .ToListAsync(cancellationToken);

    public Task<List<Empresa>> ListarEmpresasComContratoAsync(CancellationToken cancellationToken = default)
        => Contexto.Empresas
            .AsNoTracking()
            .Where(empresa => Contexto.Contracts.Any(contrato => contrato.EmpresaId == empresa.Id))
            .OrderBy(empresa => empresa.RazaoSocial)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<Guid>> ListarIdsComDocumentoAssinadoAsync(
        IReadOnlyCollection<Guid> contratoIds,
        CancellationToken cancellationToken = default)
    {
        if (contratoIds.Count == 0)
            return [];

        return await Contexto.Documents
            .Where(documento =>
                documento.ContratoId.HasValue &&
                contratoIds.Contains(documento.ContratoId.Value) &&
                documento.Origem == "ASSINATURA_AUTENTIQUE")
            .Select(documento => documento.ContratoId!.Value)
            .ToHashSetAsync(cancellationToken);
    }

    public Task<Contrato?> ObterDetalheAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts
            .Include(contrato => contrato.Prospecto)
            .Include(contrato => contrato.Cliente)
            .Include(contrato => contrato.Orcamento)
                .ThenInclude(orcamento => orcamento!.Itens)
            .Include(contrato => contrato.SolicitacoesAssinatura
                .OrderByDescending(solicitacao => solicitacao.SolicitadoEm)
                .Take(1))
                .ThenInclude(solicitacao => solicitacao.Signatarios)
            .FirstOrDefaultAsync(contrato => contrato.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaGeracaoPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Prospecto)
                .ThenInclude(p => p.Municipio).ThenInclude(m => m!.Estado)
            .Include(c => c.RepresentanteSocio).ThenInclude(s => s!.TipoRelacao)
            .Include(c => c.RepresentanteSocio).ThenInclude(s => s!.Pessoa)
                .ThenInclude(p => p.Endereco).ThenInclude(e => e!.Municipio).ThenInclude(m => m.Estado)
            .Include(c => c.Empresa)
                .ThenInclude(empresa => empresa!.Endereco)
                    .ThenInclude(endereco => endereco!.Municipio)
                        .ThenInclude(municipio => municipio.Estado)
            .Include(c => c.Orcamento)
                .ThenInclude(o => o!.Itens)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

    public async Task<Contrato?> ObterParaAssinaturaAsync(Guid contratoId, CancellationToken cancellationToken = default)
        => await Contexto.Contracts
            .Include(c => c.Cliente)
            .Include(c => c.RepresentanteSocio)
                .ThenInclude(socio => socio!.Pessoa)
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

    public Task<List<SolicitacaoAssinaturaPendente>> ListarSolicitacoesAssinaturaParaReconciliacaoAsync(
        string provedor,
        CancellationToken cancellationToken = default)
        => Contexto.AssinaturaSolicitacoes
            .IgnoreQueryFilters()
            .Where(solicitacao =>
                solicitacao.Provedor == provedor &&
                solicitacao.IdExterno != null &&
                solicitacao.Contrato.Status != StatusContrato.Cancelado &&
                ((solicitacao.ConcluidoEm == null &&
                  !Contexto.AssinaturaEventos.IgnoreQueryFilters().Any(evento =>
                      evento.AssinaturaSolicitacaoId == solicitacao.Id &&
                      evento.TipoEvento == "signature.rejected")) ||
                 (solicitacao.ConcluidoEm != null &&
                 !Contexto.Documents.IgnoreQueryFilters().Any(documento =>
                     documento.ContratoId == solicitacao.ContratoId &&
                     documento.Origem == "ASSINATURA_AUTENTIQUE"))))
            .OrderBy(solicitacao => solicitacao.SolicitadoEm)
            .Select(solicitacao => new SolicitacaoAssinaturaPendente(
                solicitacao.TenantId,
                solicitacao.IdExterno!,
                solicitacao.ContratoId))
            .ToListAsync(cancellationToken);

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

    public async Task<StatusCliente> ObterOuCriarStatusClienteAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default)
    {
        var statusLocal = Contexto.StatusClientes.Local
            .FirstOrDefault(item => item.Codigo == codigo);
        if (statusLocal is not null)
            return statusLocal;

        var status = await Contexto.StatusClientes
            .FirstOrDefaultAsync(item => item.Codigo == codigo, cancellationToken);

        if (status is not null)
            return status;

        status = new StatusCliente(codigo, descricao, ordem);
        await Contexto.StatusClientes.AddAsync(status, cancellationToken);
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
