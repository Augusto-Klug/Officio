using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IProspectoRepositorio
{
    Task<List<Socio>> ObterSociosClienteAsync(Guid clienteId, CancellationToken ct = default);
    Task<bool> SociosPossuemContratosAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<List<TipoRelacao>> ListarTiposRelacaoSocietariaAsync(CancellationToken ct = default);
    Task<TipoRelacao?> ObterTipoRelacaoSocietariaAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteComCnpjAsync(string cnpj, CancellationToken cancellationToken = default, Guid? ignorarProspectoId = null);
    Task<bool> PossuiVinculosAsync(Guid prospectoId, CancellationToken cancellationToken = default);
    Task<bool> SociosPossuemDocumentosAsync(IReadOnlyCollection<Guid> sociosIds, CancellationToken cancellationToken = default);
    void RemoverSocios(IEnumerable<Socio> socios);
    void Remover(Prospecto prospecto);
    Task<Prospecto?> ObterComSociosAsync(Guid prospectoId, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterClientePorProspectoAsync(Guid prospectoId, CancellationToken cancellationToken = default);
    Task<Empresa?> ObterEmpresaPorCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<Municipio?> ObterMunicipioPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default);
    Task<Municipio?> ObterMunicipioPorIdAsync(Guid municipioId, CancellationToken cancellationToken = default);
    Task<Estado?> ObterEstadoPorUfAsync(string uf, CancellationToken cancellationToken = default);
    Task<StatusCliente> ObterOuCriarStatusClienteAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken cancellationToken = default);
    IQueryable<Prospecto> MontarConsulta(bool rastreamento = true);
    Task<int> ContarAsync(IQueryable<Prospecto> consulta, CancellationToken cancellationToken = default);
    List<ProspectoResumo> ObterPagina(IQueryable<Prospecto> consulta, int pagina, int tamanhoPagina);
    Task AdicionarAsync(Prospecto prospecto, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Empresa empresa, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Estado estado, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Municipio municipio, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Endereco endereco, CancellationToken cancellationToken = default);
    Task AdicionarSocioAsync(Socio socio, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
