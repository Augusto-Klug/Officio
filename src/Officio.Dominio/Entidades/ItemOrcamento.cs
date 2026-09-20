using Officio.Dominio.Comum;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Entidades;

public class ItemOrcamento : EntidadeAuditavel
{
    public Guid OrcamentoId { get; private set; }
    public Guid? ItemCatalogoOrcamentoId { get; private set; }
    public string Descricao { get; private set; } = default!;
    public SecaoItemOrcamento Secao { get; private set; }
    public TipoItemOrcamento Tipo { get; private set; }
    public NaturezaCobrancaItemOrcamento NaturezaCobranca { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public int Ordem { get; private set; }
    public decimal ValorTotal => Tipo == TipoItemOrcamento.ItemMonetario
        ? Quantidade * ValorUnitario
        : 0m;

    private ItemOrcamento() { }

    public static ItemOrcamento CriarSnapshot(
        Guid tenantId,
        Guid orcamentoId,
        Guid? itemCatalogoOrcamentoId,
        string descricao,
        SecaoItemOrcamento secao,
        TipoItemOrcamento tipo,
        int quantidade,
        decimal valorUnitario,
        int ordem,
        NaturezaCobrancaItemOrcamento naturezaCobranca = NaturezaCobrancaItemOrcamento.NaoRecorrente)
    {
        if (tipo != TipoItemOrcamento.ItemMonetario && naturezaCobranca != NaturezaCobrancaItemOrcamento.NaoRecorrente)
            throw new ArgumentException("Somente itens monetarios podem compor o faturamento recorrente.", nameof(naturezaCobranca));

        return new ItemOrcamento
        {
            TenantId = tenantId,
            OrcamentoId = orcamentoId,
            ItemCatalogoOrcamentoId = itemCatalogoOrcamentoId,
            Descricao = descricao,
            Secao = secao,
            Tipo = tipo,
            NaturezaCobranca = naturezaCobranca,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario,
            Ordem = ordem
        };
    }
}
