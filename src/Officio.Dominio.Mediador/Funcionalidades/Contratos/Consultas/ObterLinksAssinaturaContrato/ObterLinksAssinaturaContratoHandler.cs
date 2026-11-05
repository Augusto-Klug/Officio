using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterLinksAssinaturaContrato;

public sealed class ObterLinksAssinaturaContratoHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterLinksAssinaturaContratoConsulta, Resultado<LinksAssinaturaContratoDto>>
{
    public Task<Resultado<LinksAssinaturaContratoDto>> Handle(
        ObterLinksAssinaturaContratoConsulta request,
        CancellationToken cancellationToken)
        => contratoServico.ObterLinksAssinaturaAsync(request.ContratoId, cancellationToken);
}
