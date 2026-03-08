using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Events;

public record ProspectConvertedEvent(Guid ProspectId, Guid ClientId, Guid TenantId) : IDomainEvent;
