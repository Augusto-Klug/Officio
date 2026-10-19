using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Servicos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class AdmissaoOcrRealIntegracaoTestes
{
    private static readonly IReadOnlyDictionary<string, string[]> TermosDocumento = new Dictionary<string, string[]>
    {
        ["IDENTIDADE"] = ["identidade", "rg", "cin"],
        ["CNH"] = ["cnh", "habilitacao", "driver"],
        ["COMPROVANTE_RESIDENCIA"] = ["resid", "endereco", "energia", "agua", "celesc", "casan", "luz", "fatura"],
        ["COMPROVANTE_ESCOLARIDADE"] = ["escolar", "diploma", "historico", "curso", "ensino", "graduacao"],
        ["TITULO_ELEITOR"] = ["titulo", "eleitor", "tse"],
        ["CERTIFICADO_RESERVISTA"] = ["reservista", "alistamento", "militar"],
        ["CARTEIRA_TRABALHO"] = ["trabalho", "ctps", "carteira"]
    };

    [OcrRealFact]
    public async Task Admissoes_ComAmostrasDisponiveisNoAzurite_ProcessamOCRReal()
    {
        using var factory = new OfficioWebApplicationFactory { UsarIntegracoesReais = true };
        var tenantId = await factory.PrepararTenantAsync();
        using (var scope = factory.Services.CreateScope())
            Assert.IsType<ServicoOcrAzure>(scope.ServiceProvider.GetRequiredService<IServicoOcr>());
        var container = factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient("documents");
        var blobs = await ListarAmostrasAsync(container);
        Assert.NotEmpty(blobs);

        var cliente = factory.CreateClient();
        var tokenAcesso = await LoginAsync(cliente);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);
        var clienteId = await CriarClienteAsync(factory, tenantId);
        var amostras = TermosDocumento.Keys
            .Select(tipo => (Tipo: tipo, Arquivo: SelecionarAmostra(blobs, tipo)))
            .Where(item => item.Arquivo is not null)
            .Select(item => (item.Tipo, Arquivo: item.Arquivo!))
            .ToList();
        var tipoSolicitado = Environment.GetEnvironmentVariable("OFFICIO_OCR_REAL_TIPO");
        if (!string.IsNullOrWhiteSpace(tipoSolicitado))
            amostras = amostras.Where(item =>
                item.Tipo.Equals(tipoSolicitado, StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.NotEmpty(amostras);
        Console.WriteLine(
            $"Azurite: {blobs.Count} arquivos; categorias com amostras: {string.Join(',', amostras.Select(item => item.Tipo))}; sem amostra identificavel: {string.Join(',', TermosDocumento.Keys.Except(amostras.Select(item => item.Tipo)))}");

        var proximaRequisicaoAzureEm = DateTimeOffset.MinValue;
        foreach (var (tipo, arquivo) in amostras)
        {
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);
            var admissaoId = await CriarAdmissaoAsync(
                cliente, clienteId, "Amostra de Integracao OCR", "11789299900", "1234567");
            var tokenFicha = await GerarLinkFichaAsync(cliente, admissaoId);
            var itemId = await ObterItemDocumentoAsync(cliente, tokenFicha, tipo);

            try
            {
                await EnviarAmostraAsync(cliente, container, tokenFicha, itemId, arquivo);
                await PrepararAdmissaoParaValidacaoAsync(factory, admissaoId);
                cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);

                await AguardarRequisicaoAzureAsync(proximaRequisicaoAzureEm);
                var validacao = await cliente.PostAsync(
                    $"/api/v1/admissoes/ValidarDocumento/{admissaoId}/itens/{itemId}", null);
                proximaRequisicaoAzureEm = DateTimeOffset.UtcNow.AddSeconds(1.1);
                Assert.Equal(HttpStatusCode.OK, validacao.StatusCode);
                var documentos = await cliente.GetFromJsonAsync<JsonArray>(
                    $"/api/v1/admissoes/ListarDocumentos/{admissaoId}");
                var documento = Assert.Single(documentos!, item => item!["id"]!.GetValue<Guid>() == itemId);
                var campos = documento!["camposExtraidos"]!.AsObject().Select(campo => campo.Key).ToArray();
                var confiancas = documento["confiancasCamposOcr"]?.AsObject()
                    .Select(campo => $"{campo.Key}:{campo.Value}").ToArray() ?? [];
                var tipoAzure = documento["camposExtraidos"]?["tipo_documento_azure"]?.GetValue<string>();
                var divergencias = documento["divergencias"]?.AsArray()
                    .Select(divergencia => divergencia?["campo"]?.GetValue<string>())
                    .Where(campo => !string.IsNullOrWhiteSpace(campo)).ToArray() ?? [];
                Console.WriteLine(
                    $"OCR_REAL tipo={tipo} tipoAzure={tipoAzure} tipoDetectado={documento["tipoDocumentoDetectado"]} statusValidacao={documento["statusValidacao"]} categoria={documento["categoriaResultadoOcr"]} http={documento["statusHttpOcr"]} codigo={documento["codigoErroOcr"]} requestId={documento["idRequisicaoOcr"]} modelo={documento["modeloOcr"]} campos={string.Join(',', campos)} divergencias={string.Join(',', divergencias)} confiancas={string.Join(',', confiancas)} comparacao=dados-sinteticos");
            }
            finally
            {
                await RemoverBlobsDaAdmissaoAsync(container, tenantId, admissaoId);
            }
        }
    }

    [OcrRealFact]
    public async Task InventarioAzurite_ContaAmostrasPorBlocoSemExporNomesDeArquivos()
    {
        using var factory = new OfficioWebApplicationFactory { UsarIntegracoesReais = true };
        var container = factory.Services.GetRequiredService<BlobServiceClient>().GetBlobContainerClient("documents");
        var blobs = await ListarAmostrasAsync(container);
        Console.WriteLine($"Azurite: {blobs.Count} arquivos PDF/JPG/PNG; {ResumirInventario(blobs)}");
        Assert.NotEmpty(blobs);
    }

    private static async Task<List<BlobItem>> ListarAmostrasAsync(BlobContainerClient container)
    {
        var arquivos = new List<BlobItem>();
        await foreach (var blob in container.GetBlobsAsync())
        {
            if (MimeType(Path.GetExtension(blob.Name)) is not null && blob.Properties.ContentLength <= 10 * 1024 * 1024)
                arquivos.Add(blob);
        }

        return arquivos;
    }

    private static BlobItem? SelecionarAmostra(IReadOnlyCollection<BlobItem> arquivos, string tipo)
    {
        var termos = TermosDocumento.GetValueOrDefault(tipo) ?? [tipo.ToLowerInvariant()];
        return arquivos
            .Where(arquivo => termos.Any(termo =>
                NormalizarBusca(arquivo.Name).Contains(termo, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(arquivo => arquivo.Properties.ContentLength)
            .FirstOrDefault();
    }

    private static async Task AguardarRequisicaoAzureAsync(DateTimeOffset proximaRequisicaoEm)
    {
        var espera = proximaRequisicaoEm - DateTimeOffset.UtcNow;
        if (espera > TimeSpan.Zero)
            await Task.Delay(espera);
    }

    private static string ResumirInventario(IReadOnlyCollection<BlobItem> arquivos)
        => string.Join(", ", TermosDocumento.Keys.Select(tipo =>
            $"{tipo}={arquivos.Count(arquivo => SelecionarAmostra([arquivo], tipo) is not null)}"));

    private static string NormalizarBusca(string valor)
    {
        var decomposicao = valor.Normalize(NormalizationForm.FormD);
        return string.Concat(decomposicao.Where(caractere =>
            CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark));
    }

    private static string? MimeType(string extensao)
        => extensao.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => null
        };

    private static async Task<Guid> CriarAdmissaoAsync(
        HttpClient cliente,
        Guid clienteId,
        string nome,
        string cpf,
        string rg)
    {
        var criacao = await cliente.PostAsJsonAsync("/api/v1/admissoes/CriarManual", new
        {
            clienteId,
            dados = new
            {
                nomeFuncionario = nome,
                cpf,
                rg,
                dataNascimento = new DateOnly(1995, 6, 15),
                sexo = "Feminino",
                telefone = "47999990000",
                dependentes = Array.Empty<object>()
            }
        });
        Assert.True(criacao.StatusCode == HttpStatusCode.Created,
            $"Criacao da admissao retornou {(int)criacao.StatusCode}: {await criacao.Content.ReadAsStringAsync()}");
        return (await criacao.Content.ReadFromJsonAsync<Guid>());
    }

    private static async Task<string> GerarLinkFichaAsync(HttpClient cliente, Guid admissaoId)
    {
        var resposta = await cliente.PostAsync($"/api/v1/admissoes/GerarLinkFicha/{admissaoId}", null);
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
    }

    private static async Task<Guid> ObterItemDocumentoAsync(HttpClient cliente, string token, string tipo)
    {
        cliente.DefaultRequestHeaders.Authorization = null;
        var resposta = await cliente.GetAsync($"/api/v1/admissoes-publicas/Obter/{token}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var ficha = await resposta.Content.ReadFromJsonAsync<JsonObject>();
        var item = ficha!["documentos"]!.AsArray().FirstOrDefault(documento =>
            documento!["tipoCodigo"]!.GetValue<string>() == tipo);
        Assert.NotNull(item);
        return item!["id"]!.GetValue<Guid>();
    }

    private static async Task EnviarAmostraAsync(
        HttpClient cliente,
        BlobContainerClient container,
        string token,
        Guid itemId,
        BlobItem arquivo)
    {
        var download = await container.GetBlobClient(arquivo.Name).DownloadContentAsync();
        using var conteudo = new ByteArrayContent(download.Value.Content.ToArray());
        conteudo.Headers.ContentType = new MediaTypeHeaderValue(MimeType(Path.GetExtension(arquivo.Name))!);
        using var formulario = new MultipartFormDataContent();
        formulario.Add(conteudo, "arquivos", Path.GetFileName(arquivo.Name));
        var upload = await cliente.PostAsync(
            $"/api/v1/admissoes-publicas/EnviarDocumento/{token}/itens/{itemId}", formulario);
        Assert.True(upload.StatusCode == HttpStatusCode.OK,
            $"Upload do bloco retornou {(int)upload.StatusCode}: {await upload.Content.ReadAsStringAsync()}");
    }

    private static async Task PrepararAdmissaoParaValidacaoAsync(
        OfficioWebApplicationFactory factory,
        Guid admissaoId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var admissao = await contexto.Admissoes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(item => item.Id == admissaoId);
        Assert.NotNull(admissao);
        var status = await contexto.StatusAdmissoes.FirstOrDefaultAsync(item =>
            item.Codigo == Officio.Dominio.Admissoes.StatusAdmissaoCodigos.AguardandoValidacao);
        if (status is null)
        {
            status = new StatusAdmissao("AGUARDANDO_VALIDACAO", "Aguardando validacao", 3);
            contexto.StatusAdmissoes.Add(status);
        }
        admissao!.MarcarPreenchimentoFuncionarioConcluido();
        admissao.AlterarStatus(status.Id);
        await contexto.SaveChangesAsync();
    }

    private static async Task RemoverBlobsDaAdmissaoAsync(
        BlobContainerClient container,
        Guid tenantId,
        Guid admissaoId)
    {
        var prefixo = $"admissoes/{tenantId}/{admissaoId}/";
        await foreach (var blob in container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefixo, CancellationToken.None))
            await container.DeleteBlobIfExistsAsync(blob.Name);
    }

    private static async Task<Guid> CriarClienteAsync(OfficioWebApplicationFactory factory, Guid tenantId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa Integracao OCR");
        var cliente = Cliente.CriarParaEmpresa(tenantId, empresa.Id);
        contexto.Empresas.Add(empresa);
        contexto.Clients.Add(cliente);
        await contexto.SaveChangesAsync();
        return cliente.Id;
    }

    private static async Task<string> LoginAsync(HttpClient cliente)
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new
        {
            email = "admin@integracao.test",
            senha = "Tenant@123"
        });
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
    }
}
