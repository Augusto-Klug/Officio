using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes;
using Officio.Infraestrutura.Integracoes.Autentique;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class AutentiqueIntegracaoClienteTestes
{
    [Fact]
    public async Task AtualizarPrazoAsync_EnviaMutationComPrazoUtc()
    {
        var handler = new CapturaRequisicaoHandler();
        var opcoes = Options.Create(new OpcoesAutentique { BaseUrl = "https://api.autentique.com.br/v2/graphql" });
        var cliente = new AutentiqueIntegracaoCliente(new ClienteIntegracao<OpcoesAutentique>(new HttpClient(handler), opcoes), opcoes);
        var prazo = new DateTime(2026, 10, 28, 12, 30, 0, DateTimeKind.Utc);

        await cliente.AtualizarPrazoAsync("doc-1", prazo, "token-teste");

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("token-teste", handler.AuthorizationParameter);
        using var json = JsonDocument.Parse(handler.Json!);
        Assert.Contains("updateDocument(id: $id, document: $document)", json.RootElement.GetProperty("query").GetString());
        var variables = json.RootElement.GetProperty("variables");
        Assert.Equal("doc-1", variables.GetProperty("id").GetString());
        Assert.Equal("2026-10-28T12:30:00.000Z", variables.GetProperty("document").GetProperty("deadline_at").GetString());
    }

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
        Assert.Equal("application/pdf", handler.ArquivoContentType);
        Assert.Equal("contrato.pdf", handler.ArquivoNome);
        Assert.NotNull(handler.Multipart);
        Assert.Contains("operations", handler.Multipart!.Keys);
        Assert.Contains("map", handler.Multipart.Keys);
        Assert.Contains("file", handler.Multipart.Keys);
        Assert.Contains("createDocument", handler.Multipart["operations"]);
        Assert.Contains("organization_id: org-1", handler.Multipart["operations"]);
        Assert.Contains("folder_id: \\u0022folder-1\\u0022", handler.Multipart["operations"]);
        Assert.DoesNotContain("\"organization_id\"", handler.Multipart["operations"]);
        using var operacoes = JsonDocument.Parse(handler.Multipart["operations"]);
        var documento = operacoes.RootElement.GetProperty("variables").GetProperty("document");
        Assert.False(documento.GetProperty("sortable").GetBoolean());
        var signatarios = operacoes.RootElement.GetProperty("variables").GetProperty("signers");
        Assert.Equal(2, signatarios.GetArrayLength());
        Assert.Equal("Representante Cliente", signatarios[0].GetProperty("name").GetString());
        Assert.Equal("12345678901", signatarios[0].GetProperty("configs").GetProperty("cpf").GetString());
        Assert.Equal("Responsavel Contabilidade", signatarios[1].GetProperty("name").GetString());
        Assert.Equal("98765432100", signatarios[1].GetProperty("configs").GetProperty("cpf").GetString());
        Assert.Contains("\"delivery_method\":\"DELIVERY_METHOD_LINK\"", handler.Multipart["operations"]);
        Assert.Equal("""{"file":["variables.file"]}""", handler.Multipart["map"]);
        Assert.Equal("%PDF-1.4 teste", handler.Multipart["file"]);
    }

    [Fact]
    public async Task CriarDocumentoAsync_ComSandbox_EnviaMutationSandbox()
    {
        var handler = new CapturaRequisicaoHandler();
        var httpClient = new HttpClient(handler);
        var opcoes = Options.Create(new OpcoesAutentique
        {
            BaseUrl = "https://api.autentique.com.br/v2/graphql",
            Sandbox = true
        });
        var clienteIntegracao = new ClienteIntegracao<OpcoesAutentique>(httpClient, opcoes);
        var cliente = new AutentiqueIntegracaoCliente(clienteIntegracao, opcoes);

        await cliente.CriarDocumentoAsync(CriarSolicitacao(), "token-teste");

        Assert.NotNull(handler.Multipart);
        Assert.Contains("createDocument(document: $document, signers: $signers, file: $file, sandbox: true)", handler.Multipart!["operations"]);
    }

    [Fact]
    public async Task ObterDocumentoAsync_UsaTipoUuidNaQuery()
    {
        var handler = new CapturaRequisicaoHandler();
        var httpClient = new HttpClient(handler);
        var opcoes = Options.Create(new OpcoesAutentique
        {
            BaseUrl = "https://api.autentique.com.br/v2/graphql"
        });
        var clienteIntegracao = new ClienteIntegracao<OpcoesAutentique>(httpClient, opcoes);
        var cliente = new AutentiqueIntegracaoCliente(clienteIntegracao, opcoes);

        await cliente.ObterDocumentoAsync("doc-1", "token-teste");

        Assert.Equal(HttpMethod.Post, handler.Metodo);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("token-teste", handler.AuthorizationParameter);
        Assert.NotNull(handler.Json);
        Assert.Contains("query RetrieveDocument($id: UUID!)", handler.Json);
        Assert.Contains("action { name }", handler.Json);
        Assert.Contains("\"id\":\"doc-1\"", handler.Json);
    }

    private static SolicitacaoAssinatura CriarSolicitacao()
        => new(
            new MemoryStream(Encoding.UTF8.GetBytes("%PDF-1.4 teste")),
            "contrato.pdf",
            [
                new DadosAssinante("Representante Cliente", "cliente@teste.com", "123.456.789-01", "contratante"),
                new DadosAssinante("Responsavel Contabilidade", "contador@teste.com", "987.654.321-00", "contratada")
            ]);

    private sealed class CapturaRequisicaoHandler : HttpMessageHandler
    {
        public HttpMethod? Metodo { get; private set; }
        public string? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? ArquivoContentType { get; private set; }
        public string? ArquivoNome { get; private set; }
        public Dictionary<string, string>? Multipart { get; private set; }
        public string? Json { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Metodo = request.Method;
            RequestUri = request.RequestUri?.ToString();
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Multipart = [];

            if (request.Content is not MultipartFormDataContent multipart)
            {
                Json = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return CriarRespostaSucesso();
            }

            foreach (var content in multipart)
            {
                var nome = content.Headers.ContentDisposition?.Name?.Trim('"');
                if (nome == "file")
                {
                    ArquivoContentType = content.Headers.ContentType?.MediaType;
                    ArquivoNome = content.Headers.ContentDisposition?.FileName?.Trim('"');
                }

                if (nome is not null)
                    Multipart[nome] = await content.ReadAsStringAsync(cancellationToken);
            }

            return CriarRespostaSucesso();
        }

        private static HttpResponseMessage CriarRespostaSucesso()
            => new(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"createDocument":{"id":"doc-1","name":"contrato.pdf","signatures":[]},"document":{"id":"doc-1","name":"contrato.pdf","files":{"signed":"https://arquivo.test/assinado.pdf"}}}}""",
                    Encoding.UTF8,
                    "application/json")
            };
    }
}
