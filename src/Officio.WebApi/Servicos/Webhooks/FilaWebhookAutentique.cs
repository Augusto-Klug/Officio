using System.Threading.Channels;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class FilaWebhookAutentique : IFilaWebhookAutentique
{
    private const int Capacidade = 500;
    private readonly Channel<string> canal = Channel.CreateBounded<string>(
        new BoundedChannelOptions(Capacidade)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TentarEnfileirar(string payload)
        => canal.Writer.TryWrite(payload);

    public IAsyncEnumerable<string> LerTodosAsync(CancellationToken cancellationToken)
        => canal.Reader.ReadAllAsync(cancellationToken);
}
