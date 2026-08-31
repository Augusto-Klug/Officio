using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Officio.Infraestrutura.Integracoes;

public sealed class RequisicaoBuilder
{
    private readonly HttpMethod metodo;
    private readonly string endpoint;
    private readonly Dictionary<string, string> headers = [];
    private HttpContent? conteudo;

    private RequisicaoBuilder(HttpMethod metodo, string endpoint)
    {
        this.metodo = metodo;
        this.endpoint = endpoint;
    }

    public static RequisicaoBuilder Post(string endpoint)
        => new(HttpMethod.Post, endpoint);

    public RequisicaoBuilder ComBearerToken(string token)
    {
        headers["Authorization"] = $"Bearer {token}";
        return this;
    }

    public RequisicaoBuilder ComJson(object valor)
    {
        conteudo = new StringContent(
            JsonSerializer.Serialize(valor, JsonOpcoes.Padrao),
            Encoding.UTF8,
            "application/json");

        return this;
    }

    public RequisicaoBuilder ComMultipart(Action<MultipartFormDataContent> configurar)
    {
        var multipart = new MultipartFormDataContent();
        configurar(multipart);
        conteudo = multipart;
        return this;
    }

    public RequisicaoIntegracao<TResposta> Criar<TResposta>()
        => new(metodo, endpoint, headers.ToDictionary(), conteudo);

    internal static class JsonOpcoes
    {
        public static readonly JsonSerializerOptions Padrao = new(JsonSerializerDefaults.Web);
    }
}
