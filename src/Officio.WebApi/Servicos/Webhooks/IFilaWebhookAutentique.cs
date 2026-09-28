using System.Diagnostics;

namespace Officio.WebApi.Servicos.Webhooks;

public interface IFilaWebhookAutentique
{
    bool TentarEnfileirar(string payload, ActivityContext contextoTrace);
    IAsyncEnumerable<WebhookAutentiqueEnfileirado> LerTodosAsync(CancellationToken cancellationToken);
}

public sealed record WebhookAutentiqueEnfileirado(string Payload, ActivityContext ContextoTrace);
