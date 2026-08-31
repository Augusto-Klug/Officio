using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;

public record ProcessarWebhookAutentiqueComando(string Payload) : IRequest<Resultado<bool>>;
