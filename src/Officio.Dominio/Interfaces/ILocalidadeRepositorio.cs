using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface ILocalidadeRepositorio
{
    Task<List<Estado>> ListarEstadosAsync(CancellationToken cancellationToken = default);
    Task<Estado?> ObterEstadoPorUfAsync(string uf, CancellationToken cancellationToken = default);
    Task<Municipio?> ObterMunicipioPorCodigoIbgeAsync(string codigoIbge, CancellationToken cancellationToken = default);
    Task<List<Municipio>> ListarMunicipiosPorEstadoAsync(Guid estadoId, CancellationToken cancellationToken = default);
    Task AdicionarEstadoAsync(Estado estado, CancellationToken cancellationToken = default);
    Task AdicionarMunicipioAsync(Municipio municipio, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
