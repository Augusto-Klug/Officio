using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ReconciliarAssinaturasAutentique;

public sealed record ReconciliarAssinaturasAutentiqueComando
    : IRequest<Resultado<ResumoReconciliacaoAssinaturas>>;
