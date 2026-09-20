using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterVisaoGeral;

public record ObterVisaoGeralDashboardConsulta : IRequest<Resultado<DashboardVisaoGeralDto>>;
