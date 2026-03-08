namespace ContaFacil.Application.Common.Models.Signature;

public record SignatureRequest(
    Stream PdfStream,
    string DocumentName,
    List<SignerInfo> Signers);

public record SignerInfo(string Nome, string Email, string? Cpf, string Tipo);

public record SignatureResult
{
    public bool Sucesso { get; init; }
    public string? EnvelopeId { get; init; }
    public List<SignerLink> SignersLinks { get; init; } = [];
    public bool FallbackRequired { get; init; }
    public string? Mensagem { get; init; }

    public static SignatureResult Success(string envelopeId, List<SignerLink> links)
        => new() { Sucesso = true, EnvelopeId = envelopeId, SignersLinks = links };

    public static SignatureResult Failure(string mensagem)
        => new() { Sucesso = false, FallbackRequired = true, Mensagem = mensagem };
}

public record SignerLink(string Email, string Link);

public record SignatureStatusResult(string Status, DateTime? SignedAt);
