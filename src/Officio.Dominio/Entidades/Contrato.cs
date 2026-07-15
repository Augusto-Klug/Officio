using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Contrato : EntidadeAuditavel
{
    public string Numero { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    public Guid? BudgetId { get; private set; }
    public StatusContrato Status { get; private set; } = StatusContrato.Rascunho;
    public decimal ValorTotal { get; private set; }
    public int DiaVencimento { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime? DataFim { get; private set; }
    public bool RenovacaoAutomatica { get; private set; }
    public string? ClausulasExtras { get; private set; }
    public string? PdfPath { get; private set; }
    public string? D4SignEnvelopeId { get; private set; }
    public DateTime? AssinadoEm { get; private set; }

    // Navigation
    public Cliente Cliente { get; private set; } = default!;
    public Orcamento? Orcamento { get; private set; }
    public ICollection<Assinatura> Assinaturas { get; private set; } = [];

    private Contrato() { }

    public static Contrato CreateFromBudget(Orcamento budget, Guid clientId, DateTime dataInicio, DateTime? dataFim, bool renovacaoAutomatica, string? clausulasExtras)
    {
        return new Contrato
        {
            TenantId = budget.TenantId,
            ClientId = clientId,
            BudgetId = budget.Id,
            ValorTotal = budget.ValorTotal,
            DiaVencimento = budget.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            RenovacaoAutomatica = renovacaoAutomatica,
            ClausulasExtras = clausulasExtras,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    public void SetPdfPath(string pdfPath) => PdfPath = pdfPath;

    public void SetD4SignEnvelope(string envelopeId)
    {
        D4SignEnvelopeId = envelopeId;
        Status = StatusContrato.AguardandoAssinatura;
    }

    public void MarkAsSigned()
    {
        Status = StatusContrato.Ativo;
        AssinadoEm = DateTime.UtcNow;
        AddDomainEvent(new EventoContratoAssinado(Id, ClientId, TenantId));
    }
}





