using System.Text.Json;
using System.Text.Json.Serialization;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public sealed record RespostaCriarDocumentoAutentique(
    DadosCriarDocumentoAutentique? Data,
    List<ErroGraphQlAutentique>? Errors);

public sealed record DadosCriarDocumentoAutentique(DocumentoAutentique? CreateDocument);

public sealed record DocumentoAutentique(string? Id, string? Name, List<AssinaturaAutentique>? Signatures);

public sealed record AssinaturaAutentique(
    [property: JsonPropertyName("public_id")] string? PublicId,
    string? Email,
    LinkAutentique? Link);

public sealed record LinkAutentique([property: JsonPropertyName("short_link")] string? ShortLink);

public sealed record ErroGraphQlAutentique(
    string? Message,
    Dictionary<string, JsonElement>? Extensions);

public sealed record RespostaObterDocumentoAutentique(
    DadosObterDocumentoAutentique? Data,
    List<ErroGraphQlAutentique>? Errors);

public sealed record DadosObterDocumentoAutentique(DocumentoArquivoAutentique? Document);

public sealed record DocumentoArquivoAutentique(string? Id, string? Name, ArquivosAutentique? Files);

public sealed record ArquivosAutentique(string? Original, string? Signed, string? Certified, string? Pades);
