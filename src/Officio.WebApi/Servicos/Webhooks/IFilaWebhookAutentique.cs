using System.Diagnostics;

namespace Officio.WebApi.Servicos.Webhooks;

public enum ResultadoRecepcaoWebhookAutentique
{
    Persistido,
    Duplicado,
    Indisponivel
}

public interface IFilaWebhookAutentique
{
    Task<ResultadoRecepcaoWebhookAutentique> PersistirAsync(
        string payload,
        ActivityContext contextoTrace,
        CancellationToken cancellationToken);

    Task<WebhookAutentiqueEnfileirado?> TentarReservarProximoAsync(CancellationToken cancellationToken);

    Task MarcarProcessadoAsync(Guid id, CancellationToken cancellationToken);

    Task RegistrarFalhaAsync(Guid id, string erro, CancellationToken cancellationToken);
}

public sealed record WebhookAutentiqueEnfileirado(
    Guid Id,
    string Payload,
    ActivityContext ContextoTrace,
    int Tentativa);
