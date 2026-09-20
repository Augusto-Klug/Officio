using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Dashboard.Consultas.ObterProjecaoReceita;

public record ObterProjecaoReceitaDashboardConsulta : IRequest<Resultado<List<DashboardProjecaoReceitaDto>>>;
