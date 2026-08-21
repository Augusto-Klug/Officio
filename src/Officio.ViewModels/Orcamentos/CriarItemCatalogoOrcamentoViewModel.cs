using Officio.ViewModels.Enumeradores;

namespace Officio.ViewModels.Orcamentos;

public class CriarItemCatalogoOrcamentoViewModel
{
    public string Descricao { get; set; } = string.Empty;
    public SecaoItemOrcamentoViewModel Secao { get; set; }
    public TipoItemOrcamentoViewModel Tipo { get; set; }
    public decimal? ValorReferencia { get; set; }
}
