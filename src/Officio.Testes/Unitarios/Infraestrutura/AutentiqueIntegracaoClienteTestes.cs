using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes;
using Officio.Infraestrutura.Integracoes.Autentique;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class AutentiqueIntegracaoClienteTestes
{
    [Fact]
    public async Task CriarDocumentoAsync_EnviaPostComBearerMultipartOperationsMapEArquivo()
    {
        var handler = new CapturaRequisicaoHandler();
        var httpClient = new HttpClient(handler);
        var opcoes = Options.Create(new OpcoesAutentique
        {
            BaseUrl = "https://api.autentique.com.br/v2/graphql",
            OrganizationId = "org-1",
            FolderId = "folder-1"
        });
        var clienteIntegracao = new ClienteIntegracao<OpcoesAutentique>(httpClient, opcoes);
        var cliente = new AutentiqueIntegracaoCliente(clienteIntegracao, opcoes);

        await cliente.CriarDocumentoAsync(CriarSolicitacao(), "token-teste");

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("https://api.autentique.com.br/v2/graphql", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("token-teste", handler.AuthorizationParameter);
        Assert.NotNull(handler.Multipart);
        Assert.Contains("operations", handler.Multipart!.Keys);
        Assert.Contains("map", handler.Multipart.Keys);
        Assert.Contains("0", handler.Multipart.Keys);
        Assert.Contains("createDocument", handler.Multipart["operations"]);
        Assert.Contains("\"organization_id\":\"org-1\"", handler.Multipart["operations"]);
        Assert.Contains("\"folder_id\":\"folder-1\"", handler.Multipart["operations"]);
        Assert.Equal("""{"0":["variables.file"]}""", handler.Multipart["map"]);
        Assert.Equal("%PDF-1.4 teste", handler.Multipart["0"]);
    }

    private static SolicitacaoAssinatura CriarSolicitacao()
        => new(
            new MemoryStream(Encoding.UTF8.GetBytes("%PDF-1.4 teste")),
            "contrato.pdf",
            [new DadosAssinante("Cliente Teste", "cliente@teste.com", "12345678901", "Cliente")]);

    private sealed class CapturaRequisicaoHandler : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public Dictionary<string, string>? Multipart { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Metodo = request.Method;
            RequestUri = request.RequestUri?.ToString();
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Multipart = [];

            Assert.IsType<MultipartFormDataContent>(request.Content);

            foreach (var content in (MultipartFormDataContent)request.Content!)
            {
                var nome = content.Headers.ContentDisposition?.Name?.Trim('"');
                if (nome is not null)
                    Multipart[nome] = await content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"createDocument":{"id":"doc-1","signatures":[]}}}""",
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }
}
