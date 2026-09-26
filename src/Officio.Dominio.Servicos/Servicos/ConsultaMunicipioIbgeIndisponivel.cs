using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class ConsultaMunicipioIbgeIndisponivel : IConsultaMunicipioIbge
{
    public Task<IReadOnlyCollection<EstadoIbge>> ListarEstadosAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<EstadoIbge>>([]);

    public Task<IReadOnlyCollection<MunicipioIbge>> ListarMunicipiosPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<MunicipioIbge>>([]);

    public Task<MunicipioIbge?> ObterPorNomeUfAsync(
        string nome,
        string uf,
        CancellationToken cancellationToken = default)
        => Task.FromResult<MunicipioIbge?>(null);
}
