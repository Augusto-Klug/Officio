using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Events;

public record DocumentValidatedEvent(Guid DocumentId, bool Valido) : IDomainEvent;
