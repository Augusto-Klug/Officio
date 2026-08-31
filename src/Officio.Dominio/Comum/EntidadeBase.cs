namespace Officio.Dominio.Comum;

public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<IEventoDominio> _domainEvents = [];
    public IReadOnlyCollection<IEventoDominio> EventosDominio => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IEventoDominio domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}





