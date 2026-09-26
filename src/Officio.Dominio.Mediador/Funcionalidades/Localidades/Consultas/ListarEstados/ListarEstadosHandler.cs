using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarEstados;

internal sealed class ListarEstadosHandler(ILocalidadeServico servico)
    : IRequestHandler<ListarEstadosConsulta, Resultado<IReadOnlyCollection<EstadoLocalidade>>>
{
    public Task<Resultado<IReadOnlyCollection<EstadoLocalidade>>> Handle(
        ListarEstadosConsulta request,
        CancellationToken cancellationToken)
        => servico.ListarEstadosAsync(cancellationToken);
}
