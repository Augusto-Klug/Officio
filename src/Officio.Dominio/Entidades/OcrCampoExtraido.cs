using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class OcrCampoExtraido : EntidadeBase
{
    public Guid OcrExecucaoId { get; private set; }
    public string CodigoCampo { get; private set; } = default!;
    public string? ValorExtraido { get; private set; }
    public decimal? Confianca { get; private set; }
    public int? Pagina { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public OcrExecucao OcrExecucao { get; private set; } = default!;

    private OcrCampoExtraido() { }

    public static OcrCampoExtraido Criar(Guid ocrExecucaoId, string codigoCampo, string? valorExtraido, decimal? confianca = null)
        => new() { OcrExecucaoId = ocrExecucaoId, CodigoCampo = codigoCampo, ValorExtraido = valorExtraido, Confianca = confianca };
}
