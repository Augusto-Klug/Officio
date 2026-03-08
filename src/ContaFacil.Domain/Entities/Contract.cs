using ContaFacil.Domain.Common;
using ContaFacil.Domain.Enums;
using ContaFacil.Domain.Events;

namespace ContaFacil.Domain.Entities;

public class Contract : AuditableEntity
{
    public string Numero { get; private set; } = default!;
    public Guid ClientId { get; private set; }
    public Guid? BudgetId { get; private set; }
    public ContractStatus Status { get; private set; } = ContractStatus.Rascunho;
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
    public Client Cliente { get; private set; } = default!;
    public Budget? Budget { get; private set; }
    public ICollection<Signature> Assinaturas { get; private set; } = [];

    private Contract() { }

    public static Contract CreateFromBudget(Budget budget, Guid clientId, DateTime dataInicio, DateTime? dataFim, bool renovacaoAutomatica, string? clausulasExtras)
    {
        return new Contract
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
        Status = ContractStatus.AguardandoAssinatura;
    }

    public void MarkAsSigned()
    {
        Status = ContractStatus.Ativo;
        AssinadoEm = DateTime.UtcNow;
        AddDomainEvent(new ContractSignedEvent(Id, ClientId, TenantId));
    }
}
