namespace Officio.Infraestrutura.Integracoes;

public sealed record RequisicaoIntegracao<TResposta>(
    HttpMethod Metodo,
    string Endpoint,
    IReadOnlyDictionary<string, string> Headers,
    HttpContent? Conteudo);
