using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ListarAlertas;

public record ListarAlertasDashboardConsulta : IRequest<Resultado<List<DashboardAlertaDto>>>;
