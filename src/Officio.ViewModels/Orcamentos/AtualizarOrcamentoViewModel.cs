namespace Officio.ViewModels.Orcamentos;

public class AtualizarOrcamentoViewModel
{
    public string? Titulo { get; set; }
    public int DiaVencimento { get; set; }
    public string? Observacoes { get; set; }
    public List<CriarItemOrcamentoViewModel> Itens { get; set; } = [];
}
