using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Notificacao : EntidadeAuditavel
{
    public Guid UserId { get; private set; }
    public string Tipo { get; private set; } = default!;
    public string Titulo { get; private set; } = default!;
    public string? Mensagem { get; private set; }
    public Guid? ReferenciaId { get; private set; }
    public bool Lida { get; private set; }
    public DateTime? LidaEm { get; private set; }

    private Notificacao() { }

    public static Notificacao Create(Guid tenantId, Guid userId, string tipo, string titulo, string? mensagem = null, Guid? referenciaId = null)
    {
        return new Notificacao
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





