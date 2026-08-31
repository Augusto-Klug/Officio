using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectos;

internal class ObterProspectosHandler(
    IProspectoServico prospectoServico) : IRequestHandler<ObterProspectosConsulta, Resultado<PagedResult<ProspectoDto>>>
{
    public async Task<Resultado<PagedResult<ProspectoDto>>> Handle(
        ObterProspectosConsulta request,
        CancellationToken ct)
        => await prospectoServico.ObterAsync(request.Filtro, ct);
}
