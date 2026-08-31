using System.Text;
using System.Text.Json;
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
                    new StringContent("""{"0":["variables.file"]}""", Encoding.UTF8, "application/json"),
                    "map");
                content.Add(new StreamContent(request.FluxoPdf), "0", request.NomeDocumento);
            })
            .Criar<RespostaCriarDocumentoAutentique>();

        return ClienteIntegracao.EnviarAsync(requisicao, cancellationToken);
    }

    private object CriarOperacoes(SolicitacaoAssinatura request)
        => new
        {
            query = """
                mutation CreateDocument($document: DocumentInput!, $signers: [SignerInput!]!, $file: Upload!) {
                  createDocument(document: $document, signers: $signers, file: $file) {
                    id
                    signatures {
                      public_id
                      signer { email }
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

    private Dictionary<string, object?> CriarDocumento(SolicitacaoAssinatura request)
    {
        var documento = new Dictionary<string, object?>
        {
            ["name"] = request.NomeDocumento
        };

        if (!string.IsNullOrWhiteSpace(options.Value.OrganizationId))
            documento["organization_id"] = options.Value.OrganizationId;

        if (!string.IsNullOrWhiteSpace(options.Value.FolderId))
            documento["folder_id"] = options.Value.FolderId;

        return documento;
    }

    private static Dictionary<string, object?> CriarSignatario(DadosAssinante assinante)
    {
        var signatario = new Dictionary<string, object?>
        {
            ["name"] = assinante.Nome,
            ["email"] = assinante.Email,
            ["action"] = "SIGN"
        };

        if (!string.IsNullOrWhiteSpace(assinante.Cpf))
            signatario["cpf"] = assinante.Cpf;

        return signatario;
    }
}
