using System.Threading.Channels;
using System.Diagnostics;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class FilaWebhookAutentique : IFilaWebhookAutentique
{
    private const int Capacidade = 500;
    private readonly Channel<WebhookAutentiqueEnfileirado> canal = Channel.CreateBounded<WebhookAutentiqueEnfileirado>(
        new BoundedChannelOptions(Capacidade)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TentarEnfileirar(string payload, ActivityContext contextoTrace)
    {
        MetricasWebhookAutentique.RegistrarEntradaNaFila();
        if (canal.Writer.TryWrite(new WebhookAutentiqueEnfileirado(payload, contextoTrace)))
            return true;

        MetricasWebhookAutentique.RegistrarSaidaDaFila();
        return false;
    }

    public async IAsyncEnumerable<WebhookAutentiqueEnfileirado> LerTodosAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in canal.Reader.ReadAllAsync(cancellationToken))
        {
            MetricasWebhookAutentique.RegistrarSaidaDaFila();
            yield return item;
        }
    }
}
