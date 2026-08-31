using Officio.Dominio.Comum;

namespace Officio.Dominio.Eventos;

public record EventoDocumentoValidado(Guid DocumentId, bool Valido) : IEventoDominio;





