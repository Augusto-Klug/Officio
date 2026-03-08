using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Events;

public record ContractSignedEvent(Guid ContractId, Guid ClientId, Guid TenantId) : IDomainEvent;
