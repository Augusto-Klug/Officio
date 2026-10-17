using Officio.Dominio.Eventos;
using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class Contrato : EntidadeAuditavel
{
    public string Numero { get; private set; } = default!;
    public Guid EmpresaId { get; private set; }
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
    public string? ClausulasExtras { get; private set; }
    public string? PdfPath { get; private set; }
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

    public static Contrato CreateFromBudget(Orcamento budget, Guid clientId, DateTime dataInicio, DateTime? dataFim, string? clausulasExtras)
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
            ClausulasExtras = clausulasExtras,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    public static Contrato CriarDeOrcamento(
        Orcamento orcamento,
        Prospecto prospecto,
        DateTime dataInicio,
        DateTime? dataFim,
        string? clausulasExtras)
    {
        if (orcamento.ProspectId != prospecto.Id)
            throw new InvalidOperationException("Prospecto nao pertence ao orcamento informado.");
        return new Contrato
        {
            TenantId = orcamento.TenantId,
            ProspectId = prospecto.Id,
            EmpresaId = prospecto.EmpresaId.GetValueOrDefault(),
            ClientId = null,
            BudgetId = orcamento.Id,
            ValorTotal = orcamento.ValorTotal,
            DiaVencimento = orcamento.DiaVencimento,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ClausulasExtras = clausulasExtras,
            Status = StatusContrato.Rascunho,
            Numero = $"CTR-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString()[..4].ToUpper()}"
        };
    }

    public void SetPdfPath(string pdfPath) => PdfPath = pdfPath;

    public void MarcarAguardandoAssinatura()
        => Status = StatusContrato.AguardandoAssinatura;

    public void Encerrar()
    {
        if (Status == StatusContrato.Ativo)
            Status = StatusContrato.Encerrado;
    }

    public void VincularCliente(Guid clientId)
    {
        if (clientId == Guid.Empty)
            throw new ArgumentException("Cliente e obrigatorio.", nameof(clientId));

        if (ClientId.HasValue && ClientId != clientId)
            throw new InvalidOperationException("Contrato ja esta vinculado a outro cliente.");

        ClientId = clientId;
    }

    public void VincularEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("Empresa e obrigatoria.", nameof(empresaId));

        if (EmpresaId != Guid.Empty && EmpresaId != empresaId)
            throw new InvalidOperationException("Contrato ja esta vinculado a outra empresa.");

        EmpresaId = empresaId;
    }

    public void MarkAsSigned(DateTime? assinadoEm = null)
    {
        if (AssinadoEm.HasValue && (Status is StatusContrato.Encerrado or StatusContrato.Cancelado))
            return;

        var jaEstavaAssinado = AssinadoEm.HasValue;
        Status = StatusContrato.Ativo;
        AssinadoEm ??= assinadoEm ?? DateTime.UtcNow;

        if (!jaEstavaAssinado)
            AddDomainEvent(new EventoContratoAssinado(Id, ClientId, TenantId));
    }
}





