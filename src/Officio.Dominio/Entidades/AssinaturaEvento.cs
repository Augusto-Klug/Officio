using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class AssinaturaEvento : EntidadeAuditavel
{
    public Guid AssinaturaSolicitacaoId { get; private set; }
    public Guid? AssinaturaSignatarioId { get; private set; }
    public string TipoEvento { get; private set; } = default!;
    public string? Payload { get; private set; }
    public DateTime OcorridoEm { get; private set; } = DateTime.UtcNow;
    public AssinaturaSolicitacao AssinaturaSolicitacao { get; private set; } = default!;
    public AssinaturaSignatario? AssinaturaSignatario { get; private set; }

    private AssinaturaEvento() { }

    public static AssinaturaEvento Criar(
        Guid tenantId,
        Guid assinaturaSolicitacaoId,
        string tipoEvento,
        string? payload = null,
        DateTime? ocorridoEm = null)
        => new()
        {
            TenantId = tenantId,
            AssinaturaSolicitacaoId = assinaturaSolicitacaoId,
            TipoEvento = tipoEvento,
            Payload = payload,
            OcorridoEm = ocorridoEm ?? DateTime.UtcNow
        };
}
