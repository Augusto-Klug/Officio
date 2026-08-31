using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Contrato : EntidadeAuditavel
{
    public string Numero { get; private set; } = default!;
    public Guid? EmpresaId { get; private set; }
    public Guid? OrcamentoVersaoAceitaId { get; private set; }
    public Guid? ModeloContratoId { get; private set; }
    public Guid? StatusContratoId { get; private set; }
    public Guid? ClientId { get; private set; }
    public Guid ProspectId { get; private set; }
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
    public Prospecto Prospecto { get; private set; } = default!;
    public Empresa? Empresa { get; private set; }
    public Cliente? Cliente { get; private set; }
    public Orcamento? Orcamento { get; private set; }
    public OrcamentoVersao? OrcamentoVersaoAceita { get; private set; }
    public ModeloContrato? ModeloContrato { get; private set; }
    public StatusContratoDominio? StatusContratoDominio { get; private set; }
    public ICollection<Assinatura> Assinaturas { get; private set; } = [];
    public ICollection<ContratoVersao> Versoes { get; private set; } = [];
    public ICollection<AssinaturaSolicitacao> SolicitacoesAssinatura { get; private set; } = [];
    public ICollection<FaturamentoMensal> FaturamentosMensais { get; private set; } = [];

    private Contrato() { }

    public static Contrato CreateFromBudget(Orcamento budget, Guid clientId, DateTime dataInicio, DateTime? dataFim, bool renovacaoAutomatica, string? clausulasExtras)
    {
        return new Contrato
        {
            TenantId = budget.TenantId,
            ProspectId = budget.ProspectId,
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

    public static Contrato CriarDeOrcamento(
        Orcamento orcamento,
        Prospecto prospecto,
        DateTime dataInicio,
        DateTime? dataFim,
        bool renovacaoAutomatica,
        string? clausulasExtras)
    {
        if (orcamento.ProspectId != prospecto.Id)
            throw new InvalidOperationException("Prospecto nao pertence ao orcamento informado.");

        return new Contrato
        {
            TenantId = orcamento.TenantId,
            ProspectId = prospecto.Id,
            EmpresaId = prospecto.EmpresaId,
            ClientId = null,
            BudgetId = orcamento.Id,
            ValorTotal = orcamento.ValorTotal,
            DiaVencimento = orcamento.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            RenovacaoAutomatica = renovacaoAutomatica,
            ClausulasExtras = clausulasExtras,
            Status = StatusContrato.Rascunho,
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

        if (ClientId.HasValue)
            AddDomainEvent(new EventoContratoAssinado(Id, ClientId.Value, TenantId));
    }
}





