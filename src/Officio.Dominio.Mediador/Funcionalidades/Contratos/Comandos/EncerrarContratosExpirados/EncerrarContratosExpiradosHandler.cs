using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EncerrarContratosExpirados;

public sealed class EncerrarContratosExpiradosHandler(IContratoServico contratoServico)
    : IRequestHandler<EncerrarContratosExpiradosComando, Resultado<int>>
{
    public Task<Resultado<int>> Handle(
        EncerrarContratosExpiradosComando request,
        CancellationToken cancellationToken)
        => contratoServico.EncerrarContratosExpiradosAsync(request.DataReferencia, cancellationToken);
}
