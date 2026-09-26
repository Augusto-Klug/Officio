using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Localidades.Consultas.ListarMunicipios;

internal sealed class ListarMunicipiosHandler(ILocalidadeServico servico)
    : IRequestHandler<ListarMunicipiosConsulta, Resultado<IReadOnlyCollection<MunicipioLocalidade>>>
{
    public Task<Resultado<IReadOnlyCollection<MunicipioLocalidade>>> Handle(
        ListarMunicipiosConsulta request,
        CancellationToken cancellationToken)
        => servico.ListarMunicipiosPorUfAsync(request.Uf, cancellationToken);
}
