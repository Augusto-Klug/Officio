using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterPdfAssinadoContrato;

public class ObterPdfAssinadoContratoHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterPdfAssinadoContratoConsulta, Resultado<ResultadoPdfAssinadoContratoDto>>
{
    public Task<Resultado<ResultadoPdfAssinadoContratoDto>> Handle(
        ObterPdfAssinadoContratoConsulta request,
        CancellationToken ct)
        => contratoServico.ObterPdfAssinadoAsync(request.ContratoId, ct);
}
