using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;

internal class GerarPdfContratoHandler(
    IContratoServico contratoServico) :
    IRequestHandler<GerarPdfContratoComando, Resultado<ResultadoPdfContratoDto>>
{
    public async Task<Resultado<ResultadoPdfContratoDto>> Handle(GerarPdfContratoComando request, CancellationToken ct)
        => await contratoServico.GerarPdfAsync(request.ContractId, ct);
}
