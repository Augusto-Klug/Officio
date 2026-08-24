using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class OrcamentoItem : EntidadeAuditavel
{
    public Guid OrcamentoVersaoId { get; private set; }
    public Guid? ItemCatalogoOrcamentoId { get; private set; }
    public Guid SecaoOrcamentoId { get; private set; }
    public string Codigo { get; private set; } = default!;
    public string Descricao { get; private set; } = default!;
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public int Ordem { get; private set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;
    public OrcamentoVersao OrcamentoVersao { get; private set; } = default!;
    public ItemCatalogoOrcamento? ItemCatalogoOrcamento { get; private set; }
    public SecaoOrcamento SecaoOrcamento { get; private set; } = default!;

    private OrcamentoItem() { }

    public static OrcamentoItem Criar(Guid tenantId, Guid orcamentoVersaoId, Guid secaoOrcamentoId, string codigo, string descricao, int quantidade, decimal valorUnitario, int ordem)
        => new()
        {
            TenantId = tenantId,
            OrcamentoVersaoId = orcamentoVersaoId,
            SecaoOrcamentoId = secaoOrcamentoId,
            Codigo = codigo,
            Descricao = descricao,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario,
            Ordem = ordem
        };
}
