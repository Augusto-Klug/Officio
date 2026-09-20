using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ValidacaoDivergencia : EntidadeBase
{
    public Guid ValidacaoDocumentoId { get; private set; }
    public string CodigoCampo { get; private set; } = default!;
    public string TipoDivergencia { get; private set; } = default!;
    public string? ValorInformado { get; private set; }
    public string? ValorExtraido { get; private set; }
    public decimal? Confianca { get; private set; }
    public string? Observacao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public ValidacaoDocumento ValidacaoDocumento { get; private set; } = default!;

    private ValidacaoDivergencia() { }

    public static ValidacaoDivergencia Criar(
        Guid validacaoDocumentoId,
        string codigoCampo,
        string tipoDivergencia,
        string? valorInformado = null,
        string? valorExtraido = null,
        decimal? confianca = null)
        => new()
        {
            ValidacaoDocumentoId = validacaoDocumentoId,
            CodigoCampo = codigoCampo,
            TipoDivergencia = tipoDivergencia,
            ValorInformado = valorInformado,
            ValorExtraido = valorExtraido,
            Confianca = confianca
        };
}
