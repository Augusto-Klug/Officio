using System.Text.Json.Serialization;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public sealed record RespostaCriarDocumentoAutentique(DadosCriarDocumentoAutentique? Data);

public sealed record DadosCriarDocumentoAutentique(DocumentoAutentique? CreateDocument);

public sealed record DocumentoAutentique(string? Id, List<AssinaturaAutentique>? Signatures);

public sealed record AssinaturaAutentique(
    [property: JsonPropertyName("public_id")] string? PublicId,
    SignatarioAutentique? Signer);

public sealed record SignatarioAutentique(string? Email);
