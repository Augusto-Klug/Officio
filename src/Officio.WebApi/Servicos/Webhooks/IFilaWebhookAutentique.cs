namespace Officio.WebApi.Servicos.Webhooks;

public interface IFilaWebhookAutentique
{
    bool TentarEnfileirar(string payload);
    IAsyncEnumerable<string> LerTodosAsync(CancellationToken cancellationToken);
}
