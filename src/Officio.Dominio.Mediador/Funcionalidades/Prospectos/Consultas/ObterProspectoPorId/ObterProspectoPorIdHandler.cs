using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Consultas.ObterProspectoPorId;

internal class ObterProspectoPorIdHandler(IProspectoServico servico)
    : IRequestHandler<ObterProspectoPorIdConsulta, Resultado<ProspectoDetalheDto>>
{
    public Task<Resultado<ProspectoDetalheDto>> Handle(ObterProspectoPorIdConsulta request, CancellationToken ct)
        => servico.ObterPorIdAsync(request.Id, ct);
}
