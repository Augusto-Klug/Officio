namespace ContaFacil.Application.Common.Models.Ocr;

public record OcrResult
{
    public bool Sucesso { get; init; }
    public Dictionary<string, string> CamposExtraidos { get; init; } = [];
    public decimal ConfiancaOcr { get; init; }
    public string? Erro { get; init; }

    public static OcrResult Success(Dictionary<string, string> campos, decimal conf)
        => new() { Sucesso = true, CamposExtraidos = campos, ConfiancaOcr = conf };

    public static OcrResult Failure(string erro)
        => new() { Sucesso = false, Erro = erro };
}
