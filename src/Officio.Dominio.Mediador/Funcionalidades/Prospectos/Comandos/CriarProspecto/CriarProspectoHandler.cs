using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

internal class CriarProspectoHandler(
    IProspectoServico prospectoServico) : IRequestHandler<CriarProspectoComando, Resultado<ProspectoCriadoDto>>
{
    public async Task<Resultado<ProspectoCriadoDto>> Handle(CriarProspectoComando request, CancellationToken ct)
        => await prospectoServico.CriarAsync(request.Requisicao, ct);
}
