using Officio.ViewModels.Enumeradores;

namespace Officio.ViewModels.Orcamentos;

public class CriarOrcamentoViewModel
{
    public Guid ProspectoId { get; set; }
    public string? Titulo { get; set; }
    public int DiaVencimento { get; set; }
    public string? Observacoes { get; set; }
    public List<CriarItemOrcamentoViewModel> Itens { get; set; } = [];
}

public class CriarItemOrcamentoViewModel
{
    public Guid? ItemCatalogoOrcamentoId { get; set; }
    public string? Descricao { get; set; }
    public SecaoItemOrcamentoViewModel? Secao { get; set; }
    public TipoItemOrcamentoViewModel? Tipo { get; set; }
    public NaturezaCobrancaItemOrcamentoViewModel? NaturezaCobranca { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public int Ordem { get; set; }
}
