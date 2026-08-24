using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class AssinaturaSolicitacao : EntidadeAuditavel
{
    private readonly List<AssinaturaSignatario> _signatarios = [];
    private readonly List<AssinaturaEvento> _eventos = [];

    public Guid ContratoId { get; private set; }
    public Guid StatusAssinaturaId { get; private set; }
    public string? Provedor { get; private set; }
    public string? IdExterno { get; private set; }
    public DateTime SolicitadoEm { get; private set; } = DateTime.UtcNow;
    public DateTime? ConcluidoEm { get; private set; }
    public Contrato Contrato { get; private set; } = default!;
    public StatusAssinatura StatusAssinatura { get; private set; } = default!;
    public IReadOnlyCollection<AssinaturaSignatario> Signatarios => _signatarios.AsReadOnly();
    public IReadOnlyCollection<AssinaturaEvento> Eventos => _eventos.AsReadOnly();

    private AssinaturaSolicitacao() { }

    public static AssinaturaSolicitacao Criar(Guid tenantId, Guid contratoId, Guid statusAssinaturaId, string? provedor = null)
        => new() { TenantId = tenantId, ContratoId = contratoId, StatusAssinaturaId = statusAssinaturaId, Provedor = provedor };
}
