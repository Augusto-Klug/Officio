namespace Officio.ViewModels.Contratos;

public class ConverterOrcamentoParaContratoViewModel
{
    public DateTime DataInicio { get; init; }
    public DateTime? DataFim { get; init; }
    public bool ConfirmarSubstituicaoContratoAtivo { get; init; }
    public string? ClausulasExtras { get; init; }
}
