using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ReconciliarAssinaturasAutentique;

public sealed class ReconciliarAssinaturasAutentiqueHandler(IContratoServico contratoServico)
    : IRequestHandler<ReconciliarAssinaturasAutentiqueComando, Resultado<ResumoReconciliacaoAssinaturas>>
{
    public Task<Resultado<ResumoReconciliacaoAssinaturas>> Handle(
        ReconciliarAssinaturasAutentiqueComando request,
        CancellationToken cancellationToken)
        => contratoServico.ReconciliarAssinaturasAutentiqueAsync(cancellationToken);
}
