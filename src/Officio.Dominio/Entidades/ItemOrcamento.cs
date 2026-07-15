using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ItemOrcamento : EntidadeAuditavel
{
    public Guid BudgetId { get; private set; }
    public string Descricao { get; private set; } = default!;
    public string? Tipo { get; private set; }
    public int Quantidade { get; private set; } = 1;
    public decimal ValorUnitario { get; private set; }

    private ItemOrcamento() { }

    public static ItemOrcamento Create(Guid tenantId, Guid budgetId, string descricao, string? tipo, int quantidade, decimal valorUnitario)
    {
        return new ItemOrcamento
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





