namespace Officio.Dominio.Interfaces;

public interface IConsultaMunicipioIbge
{
    Task<IReadOnlyCollection<EstadoIbge>> ListarEstadosAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<EstadoIbge>>([]);

    Task<IReadOnlyCollection<MunicipioIbge>> ListarMunicipiosPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<MunicipioIbge>>([]);

    Task<MunicipioIbge?> ObterPorNomeUfAsync(
        string nome,
        string uf,
        CancellationToken cancellationToken = default);
}

public sealed record EstadoIbge(string CodigoIbge, string Sigla, string Nome);
public sealed record MunicipioIbge(string CodigoIbge, string Nome, string Uf);
