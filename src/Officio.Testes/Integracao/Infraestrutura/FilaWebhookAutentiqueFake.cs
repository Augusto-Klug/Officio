using System.Collections.Concurrent;
using System.Diagnostics;
using Officio.WebApi.Servicos.Webhooks;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class FilaWebhookAutentiqueFake : IFilaWebhookAutentique
{
    private readonly ConcurrentQueue<WebhookAutentiqueEnfileirado> payloads = new();

    public bool AceitarEnfileiramento { get; set; } = true;
    public IReadOnlyCollection<string> Payloads => payloads.Select(item => item.Payload).ToArray();

    public bool TentarEnfileirar(string payload, ActivityContext contextoTrace)
    {
        if (!AceitarEnfileiramento)
            return false;

        payloads.Enqueue(new WebhookAutentiqueEnfileirado(payload, contextoTrace));
        return true;
    }

    public async IAsyncEnumerable<WebhookAutentiqueEnfileirado> LerTodosAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        yield break;
    }

    public void Limpar()
    {
        AceitarEnfileiramento = true;
        while (payloads.TryDequeue(out _)) { }
    }
}
