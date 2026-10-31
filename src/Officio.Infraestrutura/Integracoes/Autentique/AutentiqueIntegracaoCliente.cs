using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Integracoes;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public sealed class AutentiqueIntegracaoCliente(
    IClienteIntegracao<OpcoesAutentique> clienteIntegracao,
    IOptions<OpcoesAutentique> options)
    : IntegracaoBase<OpcoesAutentique>(clienteIntegracao),
        IAutentiqueIntegracaoCliente
{
    private const string EndpointGraphQl = "";

    public Task<RespostaAtualizarDocumentoAutentique?> AtualizarPrazoAsync(string documentoId, DateTime prazo, string apiToken, CancellationToken cancellationToken = default)
    {
        var requisicao = RequisicaoBuilder.Post(EndpointGraphQl).ComBearerToken(apiToken)
            .ComJson(new
            {
                query = "mutation UpdateDocument($id: UUID!, $document: UpdateDocumentInput!) { updateDocument(id: $id, document: $document) { id deadline_at } }",
                variables = new { id = documentoId, document = new { deadline_at = prazo.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") } }
            }).Criar<RespostaAtualizarDocumentoAutentique>();
        return ClienteIntegracao.EnviarAsync(requisicao, cancellationToken);
    }

    public Task<RespostaCriarDocumentoAutentique?> CriarDocumentoAsync(
        SolicitacaoAssinatura request,
        string apiToken,
        CancellationToken cancellationToken = default)
    {
        var requisicao = RequisicaoBuilder
            .Post(EndpointGraphQl)
            .ComBearerToken(apiToken)
            .ComMultipart(content =>
            {
                content.Add(
                    new StringContent(JsonSerializer.Serialize(CriarOperacoes(request), RequisicaoBuilder.JsonOpcoes.Padrao), Encoding.UTF8, "application/json"),
                    "operations");
                content.Add(
                    new StringContent("""{"file":["variables.file"]}""", Encoding.UTF8, "application/json"),
                    "map");
                var arquivo = new StreamContent(request.FluxoPdf);
                arquivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                content.Add(arquivo, "file", request.NomeDocumento);
            })
            .Criar<RespostaCriarDocumentoAutentique>();

        return ClienteIntegracao.EnviarAsync(requisicao, cancellationToken);
    }

    public Task<RespostaObterDocumentoAutentique?> ObterDocumentoAsync(
        string documentoId,
        string apiToken,
        CancellationToken cancellationToken = default)
    {
        var requisicao = RequisicaoBuilder
            .Post(EndpointGraphQl)
            .ComBearerToken(apiToken)
            .ComJson(new
            {
                query = """
                    query RetrieveDocument($id: UUID!) {
                      document(id: $id) {
                        id
                        name
                        files { original signed certified pades }
                        signatures { email action { name } signed { created_at } }
                      }
                    }
                    """,
                variables = new
                {
                    id = documentoId
                }
            })
            .Criar<RespostaObterDocumentoAutentique>();

        return ClienteIntegracao.EnviarAsync(requisicao, cancellationToken);
    }

    public Task<Stream> BaixarArquivoAsync(
        string url,
        CancellationToken cancellationToken = default)
        => ClienteIntegracao.BaixarAsync(url, cancellationToken: cancellationToken);

    private object CriarOperacoes(SolicitacaoAssinatura request)
        => new
        {
            query = $$"""
                mutation CreateDocument($document: DocumentInput!, $signers: [SignerInput!]!, $file: Upload!) {
                  createDocument(document: $document, signers: $signers, file: $file{{CriarArgumentosOpcionais()}}) {
                    id
                    name
                    signatures {
                      public_id
                      email
                      link { short_link }
                    }
                  }
                }
                """,
            variables = new
            {
                document = CriarDocumento(request),
                signers = request.Assinantes.Select(CriarSignatario).ToArray(),
                file = (object?)null
            }
        };

    private string CriarArgumentosOpcionais()
    {
        var argumentos = new List<string>();

        if (options.Value.Sandbox)
            argumentos.Add("sandbox: true");

        if (!string.IsNullOrWhiteSpace(options.Value.OrganizationId))
            argumentos.Add($"organization_id: {options.Value.OrganizationId}");

        if (!string.IsNullOrWhiteSpace(options.Value.FolderId))
            argumentos.Add($"folder_id: {JsonSerializer.Serialize(options.Value.FolderId, RequisicaoBuilder.JsonOpcoes.Padrao)}");

        return argumentos.Count == 0
            ? string.Empty
            : ", " + string.Join(", ", argumentos);
    }

    private Dictionary<string, object?> CriarDocumento(SolicitacaoAssinatura request)
        => new()
        {
            ["name"] = request.NomeDocumento,
            ["sortable"] = false
        };

    private static Dictionary<string, object?> CriarSignatario(DadosAssinante assinante)
    {
        var signatario = new Dictionary<string, object?>
        {
            ["name"] = assinante.Nome,
            ["email"] = assinante.Email,
            ["delivery_method"] = "DELIVERY_METHOD_LINK",
            ["action"] = "SIGN"
        };

        var cpf = Officio.Dominio.Comum.DocumentoFiscal.SomenteDigitos(assinante.Cpf);
        if (!string.IsNullOrWhiteSpace(cpf))
            signatario["configs"] = new { cpf };

        return signatario;
    }
}
