using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ProspectoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IProspectoRepositorio
{
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
                p.CriadoEm))
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
