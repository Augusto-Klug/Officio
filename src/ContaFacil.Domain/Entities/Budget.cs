using ContaFacil.Domain.Common;
using ContaFacil.Domain.Enums;

namespace ContaFacil.Domain.Entities;

public class Budget : AuditableEntity
{
    public int Numero { get; private set; }
    public string? Titulo { get; private set; }
    public Guid ProspectId { get; private set; }
    public decimal ValorMensalidade { get; private set; }
    public decimal ValorPorFuncionario { get; private set; }
    public int QtdFuncionarios { get; private set; }
    public decimal ValorTaxasExtras { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal ValorTotal { get; private set; }
    public int DiaVencimento { get; private set; } = 5;
    public int? VigenciaMeses { get; private set; }
    public DateTime? DataInicio { get; private set; }
    public DateTime? DataExpiracaoOrcamento { get; private set; }
    public string? Observacoes { get; private set; }
    public BudgetStatus Status { get; private set; } = BudgetStatus.Rascunho;
    public Guid? ContractId { get; private set; }

    // Navigation
    public Prospect Prospect { get; private set; } = default!;
    public ICollection<BudgetItem> Itens { get; private set; } = [];

    private Budget() { }

    public static Budget Create(
        Guid tenantId,
        Guid prospectId,
        string? titulo,
        decimal valorMensalidade,
        decimal valorPorFuncionario,
        int qtdFuncionarios,
        decimal valorTaxasExtras,
        decimal desconto,
        int diaVencimento)
    {
        var budget = new Budget
        {
            TenantId = tenantId,
            ProspectId = prospectId,
            Titulo = titulo,
            ValorMensalidade = valorMensalidade,
            ValorPorFuncionario = valorPorFuncionario,
            QtdFuncionarios = qtdFuncionarios,
            ValorTaxasExtras = valorTaxasExtras,
            Desconto = desconto,
            DiaVencimento = diaVencimento
        };

        budget.CalcularTotal();
        return budget;
    }

    public void CalcularTotal()
    {
        ValorTotal = ValorMensalidade
            + (ValorPorFuncionario * QtdFuncionarios)
            + ValorTaxasExtras
            - Desconto;
    }

    public void UpdateStatus(BudgetStatus newStatus) => Status = newStatus;

    public void MarkAsConverted(Guid contractId)
    {
        Status = BudgetStatus.Convertido;
        ContractId = contractId;
    }
}
