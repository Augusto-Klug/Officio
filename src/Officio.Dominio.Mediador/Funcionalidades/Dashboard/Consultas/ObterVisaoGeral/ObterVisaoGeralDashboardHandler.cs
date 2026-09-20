using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterVisaoGeral;

public class ObterVisaoGeralDashboardHandler(IDashboardServico dashboardServico)
    : IRequestHandler<ObterVisaoGeralDashboardConsulta, Resultado<DashboardVisaoGeralDto>>
{
    public Task<Resultado<DashboardVisaoGeralDto>> Handle(
        ObterVisaoGeralDashboardConsulta request,
        CancellationToken cancellationToken)
        => dashboardServico.ObterVisaoGeralAsync(cancellationToken);
}
