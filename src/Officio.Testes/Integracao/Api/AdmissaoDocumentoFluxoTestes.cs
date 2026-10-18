using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class AdmissaoDocumentoFluxoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task FichaPublica_UploadComOcrIndisponivelPermiteRevisaoManual()
    {
        var tenantId = await factory.PrepararTenantAsync();
        var clienteId = await CriarClienteAsync(tenantId);
        using var cliente = factory.CreateClient();
        var tokenAcesso = await LoginAsync(cliente);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);

        var criacao = await cliente.PostAsJsonAsync("/api/v1/admissoes/CriarManual", new
        {
            clienteId,
            dados = new
            {
                nomeFuncionario = "Funcionaria Integracao",
                cpf = "11789299900",
                rg = "1234567",
                dataNascimento = new DateOnly(1995, 6, 15),
                sexo = "Feminino",
                telefone = "47999990000",
                dependentes = Array.Empty<object>()
            }
        });
        Assert.True(criacao.StatusCode == HttpStatusCode.Created,
            $"Criacao da admissao retornou {(int)criacao.StatusCode}: {await criacao.Content.ReadAsStringAsync()}");
        var admissaoId = await criacao.Content.ReadFromJsonAsync<Guid>();

        var linkResposta = await cliente.PostAsync($"/api/v1/admissoes/GerarLinkFicha/{admissaoId}", null);
        Assert.Equal(HttpStatusCode.OK, linkResposta.StatusCode);
        var link = await linkResposta.Content.ReadFromJsonAsync<JsonObject>();
        var tokenFicha = link!["token"]!.GetValue<string>();

        cliente.DefaultRequestHeaders.Authorization = null;
        var fichaResposta = await cliente.GetAsync($"/api/v1/admissoes-publicas/Obter/{tokenFicha}");
        Assert.Equal(HttpStatusCode.OK, fichaResposta.StatusCode);
        var ficha = await fichaResposta.Content.ReadFromJsonAsync<JsonObject>();
        var itensParaEnviar = ficha!["documentos"]!.AsArray()
            .Where(item => item!["obrigatorio"]!.GetValue<bool>() ||
                item["tipoCodigo"]!.GetValue<string>() == "IDENTIDADE")
            .ToList();
        Guid itemId = Guid.Empty;
        foreach (var item in itensParaEnviar)
        {
            var tipoCodigo = item!["tipoCodigo"]!.GetValue<string>();
            var id = item["id"]!.GetValue<Guid>();
            if (tipoCodigo == "IDENTIDADE")
                itemId = id;

            using var conteudo = new ByteArrayContent(Encoding.ASCII.GetBytes("%PDF-1.4\nconteudo de teste\n%%EOF"));
            conteudo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            using var formulario = new MultipartFormDataContent();
            formulario.Add(conteudo, "arquivos", $"{tipoCodigo}.pdf");
            var upload = await cliente.PostAsync(
                $"/api/v1/admissoes-publicas/EnviarDocumento/{tokenFicha}/itens/{id}", formulario);
            Assert.True(upload.StatusCode == HttpStatusCode.OK,
                $"Upload de {tipoCodigo} retornou {(int)upload.StatusCode}: {await upload.Content.ReadAsStringAsync()}");
        }

        var envioConferencia = await cliente.PostAsync(
            $"/api/v1/admissoes-publicas/Concluir/{tokenFicha}", null);
        Assert.True(envioConferencia.StatusCode == HttpStatusCode.NoContent,
            $"Envio para conferencia retornou {(int)envioConferencia.StatusCode}: {await envioConferencia.Content.ReadAsStringAsync()}");

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);
        var validacao = await cliente.PostAsync(
            $"/api/v1/admissoes/ValidarDocumento/{admissaoId}/itens/{itemId}", null);
        Assert.True(validacao.StatusCode == HttpStatusCode.OK,
            $"Validacao retornou {(int)validacao.StatusCode}: {await validacao.Content.ReadAsStringAsync()}");
        var resultadoValidacao = await validacao.Content.ReadFromJsonAsync<JsonObject>();
        Assert.False(resultadoValidacao!["valido"]!.GetValue<bool>());
        Assert.True(resultadoValidacao["fallbackManual"]!.GetValue<bool>());

        var revisao = await cliente.PostAsJsonAsync(
            $"/api/v1/admissoes/AprovarDocumento/{admissaoId}/itens/{itemId}",
            new { observacao = "Documento conferido visualmente no teste de integracao." });
        Assert.Equal(HttpStatusCode.OK, revisao.StatusCode);

        var documentos = await cliente.GetAsync($"/api/v1/admissoes/ListarDocumentos/{admissaoId}");
        Assert.Equal(HttpStatusCode.OK, documentos.StatusCode);
        var lista = await documentos.Content.ReadFromJsonAsync<JsonArray>();
        var documentoRevisado = Assert.Single(lista!, item =>
            item!["id"]!.GetValue<Guid>() == itemId);
        Assert.Equal("APROVADO", documentoRevisado!["statusValidacao"]!.GetValue<string>());
    }

    private async Task<Guid> CriarClienteAsync(Guid tenantId)
    {
        using var scope = factory.Services.CreateScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa de Teste");
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
