using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class RegistroAuditoria : EntidadeBase
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Entidade { get; set; } = default!;
    public Guid EntidadeId { get; set; }
    public string Acao { get; set; } = default!;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}





