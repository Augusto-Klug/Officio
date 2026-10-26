using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterSignatariosContrato;

public sealed class ObterSignatariosContratoHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterSignatariosContratoConsulta, Resultado<SignatariosContratoDto>>
{
    public Task<Resultado<SignatariosContratoDto>> Handle(
        ObterSignatariosContratoConsulta request,
        CancellationToken cancellationToken)
        => contratoServico.ObterSignatariosAsync(request.ContratoId, cancellationToken);
}
