using MediatR;

namespace Officio.Testes.Dubles;

internal sealed class PublicadorEventosFake : IPublisher
{
    public List<INotification> Notificacoes { get; } = [];

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        Notificacoes.Add((INotification)notification);
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        Notificacoes.Add(notification);
        return Task.CompletedTask;
    }
}
