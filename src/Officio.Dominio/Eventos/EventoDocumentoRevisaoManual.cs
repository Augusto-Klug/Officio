using Officio.Dominio.Comum;

namespace Officio.Dominio.Eventos;

public record EventoDocumentoRevisaoManual(Guid DocumentId, Guid TenantId) : IEventoDominio;
