using Officio.Dominio.Comum;

namespace Officio.Dominio.Eventos;

public record EventoOrcamentoAceito(Guid OrcamentoId, Guid ProspectId, Guid TenantId) : IEventoDominio;
