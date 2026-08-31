namespace Officio.Dominio.Comum;

public abstract class EntidadeAuditavel : EntidadeBase
{
    public Guid TenantId { get; protected set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public Guid? CriadoPorId { get; private set; }
}





