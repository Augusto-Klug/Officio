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

    public void DefinirIdentificadorExterno(string idExterno)
        => IdExterno = idExterno;

    public void AlterarStatus(Guid statusAssinaturaId)
        => StatusAssinaturaId = statusAssinaturaId;

    public void MarcarConcluida(Guid statusAssinaturaId, DateTime concluidoEm)
    {
        StatusAssinaturaId = statusAssinaturaId;
        ConcluidoEm = concluidoEm;
    }

    public void AdicionarSignatario(string nome, string email, string papel, int ordem, string? cpf = null)
        => _signatarios.Add(AssinaturaSignatario.Criar(TenantId, Id, nome, email, papel, ordem, cpf));

    public void AdicionarEvento(string tipoEvento, string? payload = null, DateTime? ocorridoEm = null)
        => _eventos.Add(AssinaturaEvento.Criar(TenantId, Id, tipoEvento, payload, ocorridoEm));
}
