using System.Collections.Concurrent;
using System.Diagnostics;
using Officio.WebApi.Servicos.Webhooks;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class FilaWebhookAutentiqueFake : IFilaWebhookAutentique
{
    private readonly ConcurrentQueue<WebhookAutentiqueEnfileirado> payloads = new();

    public bool AceitarEnfileiramento { get; set; } = true;
    public IReadOnlyCollection<string> Payloads => payloads.Select(item => item.Payload).ToArray();

    public Task<ResultadoRecepcaoWebhookAutentique> PersistirAsync(
        string payload,
        ActivityContext contextoTrace,
        CancellationToken cancellationToken)
    {
        if (!AceitarEnfileiramento)
            return Task.FromResult(ResultadoRecepcaoWebhookAutentique.Indisponivel);

        payloads.Enqueue(new WebhookAutentiqueEnfileirado(Guid.NewGuid(), payload, contextoTrace, 0));
        return Task.FromResult(ResultadoRecepcaoWebhookAutentique.Persistido);
    }

    public Task<WebhookAutentiqueEnfileirado?> TentarReservarProximoAsync(CancellationToken cancellationToken)
        => Task.FromResult<WebhookAutentiqueEnfileirado?>(null);

    public Task MarcarProcessadoAsync(Guid id, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RegistrarFalhaAsync(Guid id, string erro, CancellationToken cancellationToken) => Task.CompletedTask;

    public void Limpar()
    {
        AceitarEnfileiramento = true;
        while (payloads.TryDequeue(out _)) { }
    }
}
