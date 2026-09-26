using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarEstados;

public sealed record ListarEstadosConsulta
    : IRequest<Resultado<IReadOnlyCollection<EstadoLocalidade>>>;
