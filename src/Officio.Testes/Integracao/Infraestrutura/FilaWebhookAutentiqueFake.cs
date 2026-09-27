using System.Collections.Concurrent;
using Officio.WebApi.Servicos.Webhooks;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class FilaWebhookAutentiqueFake : IFilaWebhookAutentique
{
    private readonly ConcurrentQueue<string> payloads = new();

    public bool AceitarEnfileiramento { get; set; } = true;
    public IReadOnlyCollection<string> Payloads => payloads.ToArray();

    public bool TentarEnfileirar(string payload)
    {
        if (!AceitarEnfileiramento)
            return false;

        payloads.Enqueue(payload);
        return true;
    }

    public async IAsyncEnumerable<string> LerTodosAsync(
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
