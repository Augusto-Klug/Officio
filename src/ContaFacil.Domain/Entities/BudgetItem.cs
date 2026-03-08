using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class BudgetItem : AuditableEntity
{
    public Guid BudgetId { get; private set; }
    public string Descricao { get; private set; } = default!;
    public string? Tipo { get; private set; }
    public int Quantidade { get; private set; } = 1;
    public decimal ValorUnitario { get; private set; }

    private BudgetItem() { }

    public static BudgetItem Create(Guid tenantId, Guid budgetId, string descricao, string? tipo, int quantidade, decimal valorUnitario)
    {
        return new BudgetItem
        {
            TenantId = tenantId,
            BudgetId = budgetId,
            Descricao = descricao,
            Tipo = tipo,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario
        };
    }
}
