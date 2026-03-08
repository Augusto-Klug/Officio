using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? UserId { get; set; }
    public string Entidade { get; set; } = default!;
    public Guid EntidadeId { get; set; }
    public string Acao { get; set; } = default!;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
