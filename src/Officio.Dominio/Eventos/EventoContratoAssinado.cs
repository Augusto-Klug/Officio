using Officio.Dominio.Comum;

namespace Officio.Dominio.Eventos;

public record EventoContratoAssinado(Guid ContractId, Guid ClientId, Guid TenantId) : IEventoDominio;





