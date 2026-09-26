using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface ILocalidadeServico
{
    Task<Resultado<IReadOnlyCollection<EstadoLocalidade>>> ListarEstadosAsync(
        CancellationToken cancellationToken = default);

    Task<Resultado<IReadOnlyCollection<MunicipioLocalidade>>> ListarMunicipiosPorUfAsync(
        string uf,
        CancellationToken cancellationToken = default);

    Task<Resultado<EnderecoCepLocalidade?>> ConsultarCepAsync(
        string cep,
        CancellationToken cancellationToken = default);
}
