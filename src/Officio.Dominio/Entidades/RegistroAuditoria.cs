using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class RegistroAuditoria : EntidadeBase
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Entidade { get; private set; } = default!;
    public Guid EntidadeId { get; private set; }
    public string Acao { get; private set; } = default!;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private RegistroAuditoria() { }

    public static RegistroAuditoria Criar(
        Guid tenantId,
        Guid? usuarioId,
        string entidade,
        Guid entidadeId,
        string acao)
        => new()
        {
            TenantId = tenantId,
            UserId = usuarioId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao
        };
}





