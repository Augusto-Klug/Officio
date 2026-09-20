using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IDashboardServico
{
    Task<Resultado<DashboardVisaoGeralDto>> ObterVisaoGeralAsync(CancellationToken cancellationToken = default);
    Task<Resultado<List<DashboardAlertaDto>>> ListarAlertasAsync(CancellationToken cancellationToken = default);
    Task<Resultado<List<DashboardProjecaoReceitaDto>>> ObterProjecaoReceitaAsync(CancellationToken cancellationToken = default);
}
