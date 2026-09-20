using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ListarAlertas;

public class ListarAlertasDashboardHandler(IDashboardServico dashboardServico)
    : IRequestHandler<ListarAlertasDashboardConsulta, Resultado<List<DashboardAlertaDto>>>
{
    public Task<Resultado<List<DashboardAlertaDto>>> Handle(
        ListarAlertasDashboardConsulta request,
        CancellationToken cancellationToken)
        => dashboardServico.ListarAlertasAsync(cancellationToken);
}
