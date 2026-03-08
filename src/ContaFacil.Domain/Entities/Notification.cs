using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class Notification : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Tipo { get; private set; } = default!;
    public string Titulo { get; private set; } = default!;
    public string? Mensagem { get; private set; }
    public Guid? ReferenciaId { get; private set; }
    public bool Lida { get; private set; }
    public DateTime? LidaEm { get; private set; }

    private Notification() { }

    public static Notification Create(Guid tenantId, Guid userId, string tipo, string titulo, string? mensagem = null, Guid? referenciaId = null)
    {
        return new Notification
        {
            TenantId = tenantId,
            UserId = userId,
            Tipo = tipo,
            Titulo = titulo,
            Mensagem = mensagem,
            ReferenciaId = referenciaId
        };
    }

    public void MarcarComoLida()
    {
        Lida = true;
        LidaEm = DateTime.UtcNow;
    }
}
