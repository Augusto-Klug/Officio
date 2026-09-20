using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterProjecaoReceita;

public class ObterProjecaoReceitaDashboardHandler(IDashboardServico dashboardServico)
    : IRequestHandler<ObterProjecaoReceitaDashboardConsulta, Resultado<List<DashboardProjecaoReceitaDto>>>
{
    public Task<Resultado<List<DashboardProjecaoReceitaDto>>> Handle(
        ObterProjecaoReceitaDashboardConsulta request,
        CancellationToken cancellationToken)
        => dashboardServico.ObterProjecaoReceitaAsync(cancellationToken);
}
