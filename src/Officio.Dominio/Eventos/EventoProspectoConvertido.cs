using Officio.Dominio.Comum;

namespace Officio.Dominio.Eventos;

public record EventoProspectoConvertido(Guid ProspectId, Guid ClientId, Guid TenantId) : IEventoDominio;





