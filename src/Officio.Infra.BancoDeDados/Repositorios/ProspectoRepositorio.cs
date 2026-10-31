using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ProspectoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IProspectoRepositorio
{
    public async Task<IAsyncDisposable> BloquearFluxoContratosAsync(Guid prospectoId, CancellationToken cancellationToken = default)
    {
        if (!Contexto.Database.IsSqlServer())
            return BloqueioNulo.Instancia;

        await Contexto.Database.OpenConnectionAsync(cancellationToken);
        var connection = (SqlConnection)Contexto.Database.GetDbConnection();
        try
        {
            var recurso = $"Officio:ContratoProspecto:{prospectoId:D}";
            await using var command = connection.CreateCommand();
            command.CommandText = "DECLARE @resultado int; EXEC @resultado = sp_getapplock @Resource=@recurso, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=30000; SELECT @resultado;";
            command.Parameters.Add("@recurso", SqlDbType.NVarChar, 255).Value = recurso;
            var resultado = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            if (resultado < 0)
                throw new TimeoutException("Outra operacao de contrato esta em andamento. Tente novamente.");
            return new BloqueioSql(Contexto, connection, recurso);
        }
        catch
        {
            await Contexto.Database.CloseConnectionAsync();
            throw;
        }
    }

    private sealed class BloqueioSql(ContextoAplicacao contexto, SqlConnection connection, string recurso) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "EXEC sp_releaseapplock @Resource=@recurso, @LockOwner='Session';";
                command.Parameters.Add("@recurso", SqlDbType.NVarChar, 255).Value = recurso;
                await command.ExecuteNonQueryAsync();
            }
            catch
            {
                SqlConnection.ClearPool(connection);
            }
            finally { await contexto.Database.CloseConnectionAsync(); }
        }
    }

    private sealed class BloqueioNulo : IAsyncDisposable
    {
        public static BloqueioNulo Instancia { get; } = new();
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
    public Task<bool> SociosPossuemContratosAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
        => Contexto.Contracts.AnyAsync(c => c.RepresentanteSocioId.HasValue && ids.Contains(c.RepresentanteSocioId.Value), ct);
    public Task<List<Socio>> ObterSociosClienteAsync(Guid clienteId, CancellationToken ct = default)
        => Contexto.Socios.Where(s => s.ClientId == clienteId).Include(s => s.TipoRelacao)
            .Include(s => s.Pessoa).ThenInclude(p => p.Endereco).ThenInclude(e => e!.Municipio).ThenInclude(m => m.Estado)
            .Include(s => s.Pessoa).ThenInclude(p => p.Endereco).ThenInclude(e => e!.Estado).ToListAsync(ct);
    public Task<List<TipoRelacao>> ListarTiposRelacaoSocietariaAsync(CancellationToken ct = default)
        => Contexto.TiposRelacao.Where(t => t.Ativo && (t.Codigo == "SOCIO" || t.Codigo == "SOCIO_ADMINISTRADOR"))
            .OrderBy(t => t.Codigo).ToListAsync(ct);
    public Task<TipoRelacao?> ObterTipoRelacaoSocietariaAsync(Guid id, CancellationToken ct = default)
        => Contexto.TiposRelacao.FirstOrDefaultAsync(t => t.Id == id && t.Ativo &&
            (t.Codigo == "SOCIO" || t.Codigo == "SOCIO_ADMINISTRADOR"), ct);
    public async Task<bool> ExisteComCnpjAsync(string cnpj, CancellationToken cancellationToken = default, Guid? ignorarProspectoId = null)
        => await VerificarExistenciaAsync<Prospecto>(
            p => (!ignorarProspectoId.HasValue || p.Id != ignorarProspectoId.Value) && p.Cnpj != null && p.Cnpj
                .Replace(".", "")
                .Replace("/", "")
                .Replace("-", "") == cnpj,
            cancellationToken);

    public async Task<Prospecto?> ObterComSociosAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => await Contexto.Prospects
            .Include(p => p.Socios).ThenInclude(s => s.TipoRelacao)
            .Include(p => p.Socios).ThenInclude(s => s.Pessoa).ThenInclude(p => p.Endereco).ThenInclude(e => e!.Municipio).ThenInclude(m => m.Estado)
            .Include(p => p.Socios).ThenInclude(s => s.Pessoa).ThenInclude(p => p.Endereco).ThenInclude(e => e!.Estado)
            .Include(p => p.Municipio).ThenInclude(m => m!.Estado)
            .FirstOrDefaultAsync(p => p.Id == prospectoId, cancellationToken);

    public async Task<bool> PossuiVinculosAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => await Contexto.Budgets.AnyAsync(x => x.ProspectId == prospectoId, cancellationToken)
            || await Contexto.Contracts.AnyAsync(x => x.ProspectId == prospectoId, cancellationToken)
            || await Contexto.Clients.AnyAsync(x => x.ProspectOrigemId == prospectoId, cancellationToken)
            || await Contexto.Documents.AnyAsync(x => x.ProspectId == prospectoId, cancellationToken)
            || await Contexto.FormulariosAbertura.AnyAsync(x => x.ProspectId == prospectoId, cancellationToken)
            || await SociosPossuemDocumentosAsync(
                await Contexto.Socios.Where(x => x.ProspectId == prospectoId).Select(x => x.Id).ToListAsync(cancellationToken),
                cancellationToken);

    public Task<bool> PossuiContratoAguardandoAssinaturaAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts.AnyAsync(c => c.ProspectId == prospectoId && c.Status == StatusContrato.AguardandoAssinatura, cancellationToken);

    public Task<List<Contrato>> ListarContratosCancelaveisAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => Contexto.Contracts.Include(c => c.SolicitacoesAssinatura).ThenInclude(s => s.Signatarios)
            .Where(c => c.ProspectId == prospectoId && (c.Status == StatusContrato.Rascunho || c.Status == StatusContrato.AguardandoAssinatura))
            .ToListAsync(cancellationToken);

    public Task<Empresa?> ObterEmpresaPorIdAsync(Guid empresaId, CancellationToken cancellationToken = default)
        => Contexto.Empresas.Include(e => e.Endereco).FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken);

    public IQueryable<Prospecto> FiltrarAguardandoAssinatura(IQueryable<Prospecto> consulta, bool aguardando)
    {
        var contratos = Contexto.Contracts.Where(c => c.Status == StatusContrato.AguardandoAssinatura);
        return aguardando
            ? consulta.Where(p => p.Status != StatusProspecto.Convertido && p.ClientId == null && p.ConvertidoEm == null && contratos.Any(c => c.ProspectId == p.Id && c.TenantId == p.TenantId))
            : consulta.Where(p => p.Status == StatusProspecto.Convertido || p.ClientId != null || p.ConvertidoEm != null || !contratos.Any(c => c.ProspectId == p.Id && c.TenantId == p.TenantId));
    }

    public Task<bool> SociosPossuemDocumentosAsync(IReadOnlyCollection<Guid> sociosIds, CancellationToken cancellationToken = default)
        => Contexto.Documents.AnyAsync(x => x.SocioId.HasValue && sociosIds.Contains(x.SocioId.Value), cancellationToken);

    public void RemoverSocios(IEnumerable<Socio> socios) => Contexto.Socios.RemoveRange(socios);

    public void Remover(Prospecto prospecto)
    {
        Contexto.ProspectoPessoas.RemoveRange(Contexto.ProspectoPessoas.Where(x => x.ProspectId == prospecto.Id));
        Contexto.Socios.RemoveRange(prospecto.Socios);
        Contexto.Prospects.Remove(prospecto);
    }

    public Task<Cliente?> ObterClientePorProspectoAsync(
        Guid prospectoId,
        CancellationToken cancellationToken = default)
        => Contexto.Clients.FirstOrDefaultAsync(
            cliente => cliente.ProspectOrigemId == prospectoId,
            cancellationToken);

    public Task<Empresa?> ObterEmpresaPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        => Contexto.Empresas
            .Include(empresa => empresa.Endereco)
                .ThenInclude(endereco => endereco!.Municipio)
                    .ThenInclude(municipio => municipio.Estado)
            .FirstOrDefaultAsync(empresa => empresa.Cnpj == cnpj, cancellationToken);

    public Task<Municipio?> ObterMunicipioPorCodigoIbgeAsync(
        string codigoIbge,
        CancellationToken cancellationToken = default)
        => Contexto.Municipios
            .Include(municipio => municipio.Estado)
            .FirstOrDefaultAsync(
                municipio => municipio.CodigoIbge == codigoIbge,
                cancellationToken);

    public Task<Municipio?> ObterMunicipioPorIdAsync(
        Guid municipioId,
        CancellationToken cancellationToken = default)
        => Contexto.Municipios.FirstOrDefaultAsync(
            municipio => municipio.Id == municipioId,
            cancellationToken);

    public Task<Estado?> ObterEstadoPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default)
        => Contexto.Estados.FirstOrDefaultAsync(
            estado => estado.Sigla == uf,
            cancellationToken);

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

    public IQueryable<Prospecto> MontarConsulta(bool rastreamento = true)
        => MontarConsulta<Prospecto>(rastreamento);

    public async Task<int> ContarAsync(IQueryable<Prospecto> consulta, CancellationToken cancellationToken = default)
        => await consulta.CountAsync(cancellationToken);

    public List<ProspectoResumo> ObterPagina(IQueryable<Prospecto> consulta, int pagina, int tamanhoPagina)
        => consulta
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(p => new ProspectoResumo(
                p.Id,
                p.RazaoSocial,
                p.Cnpj,
                p.Status.ToString(),
                p.UsuarioResponsavel != null ? p.UsuarioResponsavel.Nome : null,
                p.CriadoEm,
                (p.Status == StatusProspecto.EmNegociacao || p.Status == StatusProspecto.PropostaEnviada)
                    && p.ClientId == null && p.ConvertidoEm == null,
                p.Status == StatusProspecto.EmNegociacao && p.ClientId == null && p.ConvertidoEm == null && p.EmpresaId == null
                    && !Contexto.Budgets.Any(o => o.ProspectId == p.Id && o.TenantId == p.TenantId)
                    && !Contexto.Contracts.Any(c => c.ProspectId == p.Id && c.TenantId == p.TenantId)
                    && !Contexto.Clients.Any(c => c.ProspectOrigemId == p.Id && c.TenantId == p.TenantId)
                    && !Contexto.FormulariosAbertura.Any(f => f.ProspectId == p.Id)
                    && !Contexto.Documents.Any(d => d.TenantId == p.TenantId && (d.ProspectId == p.Id ||
                        (d.SocioId != null && Contexto.Socios.Any(s => s.Id == d.SocioId && s.ProspectId == p.Id && s.TenantId == p.TenantId)))),
                p.Status != StatusProspecto.Convertido && p.ClientId == null && p.ConvertidoEm == null && Contexto.Contracts.Any(c => c.ProspectId == p.Id && c.TenantId == p.TenantId && c.Status == StatusContrato.AguardandoAssinatura)))
            .ToList();

    public async Task AdicionarAsync(Prospecto prospecto, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Prospecto>(prospecto, cancellationToken);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Cliente>(cliente, cancellationToken);

    public async Task AdicionarAsync(Empresa empresa, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Empresa>(empresa, cancellationToken);

    public async Task AdicionarAsync(Estado estado, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Estado>(estado, cancellationToken);

    public async Task AdicionarAsync(Municipio municipio, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Municipio>(municipio, cancellationToken);

    public async Task AdicionarAsync(Endereco endereco, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Endereco>(endereco, cancellationToken);

    public async Task AdicionarSocioAsync(Socio socio, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Socio>(socio, cancellationToken);
}
