namespace Officio.ViewModels.Contratos;

public class ConverterOrcamentoParaContratoViewModel
{
    public DateTime DataInicio { get; init; }
    public int DuracaoAnos { get; init; } = 4;
    public DateOnly? CompetenciaInicialHonorarios { get; init; }
    public bool ConfirmarSubstituicaoContratoAtivo { get; init; }
    public string? ClausulasExtras { get; init; }
    public Guid? RepresentanteSocioId { get; init; }
}
