using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

public class ProcessarWebhookAutentiqueHandler(IContratoServico contratoServico)
    : IRequestHandler<ProcessarWebhookAutentiqueComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        ProcessarWebhookAutentiqueComando request,
        CancellationToken ct)
        => contratoServico.ProcessarWebhookAutentiqueAsync(request.Payload, ct);
}
