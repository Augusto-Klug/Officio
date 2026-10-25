using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class ContabilidadeFluxoTestes(OfficioWebApplicationFactory factory) : IClassFixture<OfficioWebApplicationFactory>
{
    private const string Rota = "/api/v1/contabilidade";

    [Fact]
    public async Task Obter_RetornaApenasParceiroDoTenantAutenticado()
    {
        await factory.PrepararTenantAsync();
        await factory.AdicionarTenantAsync("88777666000155", "outro@teste.test", "Outro@123");
        using var cliente = await LoginAsync();
        var dados = await cliente.GetFromJsonAsync<DadosContabilidadeDto>(Rota);
        Assert.Equal("99888777000166", dados!.Cnpj);
        Assert.Null(dados.Endereco);
        Assert.Null(dados.ResponsavelTecnico);
        var tentativa = await cliente.GetAsync($"{Rota}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, tentativa.StatusCode);
    }

    [Fact]
    public async Task Atualizar_CriaEAtualizaEnderecosEResponsavelSemDuplicarRegistros()
    {
        var tenant = await factory.PrepararTenantAsync();
        await PrepararMunicipioAsync();
        using var cliente = await LoginAsync();
        var inicial = Dados();
        var resposta = await cliente.PutAsJsonAsync(Rota, inicial);
        Assert.True(resposta.IsSuccessStatusCode, await resposta.Content.ReadAsStringAsync());
        var salvo = await resposta.Content.ReadFromJsonAsync<DadosContabilidadeDto>();
        Assert.Equal("11222333000144", salvo!.Cnpj);
        Assert.Equal("Contabilidade Atualizada", salvo.RazaoSocial);
        Assert.Equal("SP-123", salvo.Crc);
        Assert.Equal("11999998888", salvo.Telefone);
        Assert.Equal("12345678901", salvo.ResponsavelTecnico!.Cpf);
        Assert.Equal("Rua do Escritório", salvo.Endereco!.Logradouro);
        Assert.Equal("São Paulo", salvo.Endereco.Cidade);
        Assert.Equal("Rua Residencial", salvo.ResponsavelTecnico.Endereco!.Logradouro);

        Guid enderecoId, tecnicoId, enderecoTecnicoId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var p = await db.Parceiros.SingleAsync(p => p.Id == tenant);
            var r = await db.Set<ResponsavelTecnico>().IgnoreQueryFilters().SingleAsync();
            enderecoId = p.EnderecoId!.Value; tecnicoId = r.Id; enderecoTecnicoId = r.EnderecoId!.Value;
        }
        var atualizado = inicial with {
            RazaoSocial = "Novo nome", Endereco = inicial.Endereco! with { Numero = "200", Bairro = "Centro" },
            ResponsavelTecnico = inicial.ResponsavelTecnico! with {
                Nome = "Nova responsável", Profissao = "Contadora", Crc = "SP-999",
                Endereco = inicial.ResponsavelTecnico.Endereco! with { Numero = "300" } }
        };
        resposta = await cliente.PutAsJsonAsync(Rota, atualizado);
        Assert.True(resposta.IsSuccessStatusCode, await resposta.Content.ReadAsStringAsync());
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var p = await db.Parceiros.Include(p => p.Endereco).SingleAsync(p => p.Id == tenant);
            var r = await db.Set<ResponsavelTecnico>().IgnoreQueryFilters().Include(r => r.Endereco).SingleAsync();
            Assert.Equal(enderecoId, p.EnderecoId);
            Assert.Equal(tecnicoId, r.Id);
            Assert.Equal(enderecoTecnicoId, r.EnderecoId);
            Assert.Equal("200", p.Endereco!.Numero);
            Assert.Equal("300", r.Endereco!.Numero);
            Assert.Equal("Nova responsável", r.Nome);
            Assert.Equal("Contadora", r.Profissao);
            Assert.Equal("SP-999", r.Crc);
            Assert.Equal(tenant, r.TenantId);
            Assert.Equal(2, await db.Enderecos.CountAsync());
        }
        var leitura = await cliente.GetFromJsonAsync<DadosContabilidadeDto>(Rota);
        Assert.Equal("Novo nome", leitura!.RazaoSocial);
        Assert.Equal("300", leitura.ResponsavelTecnico!.Endereco!.Numero);
    }

    [Fact]
    public async Task Atualizar_IgnoraIdsExternosENaoAlteraOutroTenant()
    {
        await factory.PrepararTenantAsync();
        var outro = await factory.AdicionarTenantAsync("88777666000155", "outro@teste.test", "Outro@123");
        using var cliente = await LoginAsync();
        var payload = new { parceiroId = outro, tenantId = outro, razaoSocial = "Somente o atual", cnpj = "11222333000144" };
        var resposta = await cliente.PutAsJsonAsync(Rota, payload);
        Assert.True(resposta.IsSuccessStatusCode, await resposta.Content.ReadAsStringAsync());
        using var clienteOutro = await LoginAsync("outro@teste.test", "Outro@123");
        var dados = await clienteOutro.GetFromJsonAsync<DadosContabilidadeDto>(Rota);
        Assert.Equal("88777666000155", dados!.Cnpj);
        Assert.NotEqual("Somente o atual", dados.RazaoSocial);
        Assert.Null(dados.ResponsavelTecnico);
    }

    [Theory]
    [InlineData("Comercial")]
    [InlineData("Operacional")]
    [InlineData("Financeiro")]
    [InlineData("Consulta")]
    [InlineData("PlatformAdmin")]
    public async Task Endpoint_BloqueiaPerfisDiferentesDeAdmin(string perfil)
    {
        await factory.PrepararTenantAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var usuario = await db.Users.IgnoreQueryFilters().SingleAsync(u => u.Email == "admin@integracao.test");
            usuario.AlterarPerfil(perfil);
            await db.SaveChangesAsync();
        }
        using var cliente = await LoginAsync();
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.GetAsync(Rota)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await cliente.PutAsJsonAsync(Rota, Dados())).StatusCode);
    }

    [Fact]
    public async Task Endpoint_SemAutenticacao_Retorna401()
    {
        using var cliente = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.GetAsync(Rota)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await cliente.PutAsJsonAsync(Rota, Dados())).StatusCode);
    }

    [Fact]
    public async Task Atualizar_RejeitaCnpjDuplicadoEDocumentosInvalidosSemPersistir()
    {
        await factory.PrepararTenantAsync();
        await factory.AdicionarTenantAsync("88777666000155", "outro@teste.test", "Outro@123");
        using var cliente = await LoginAsync();
        foreach (var dados in new[] {
            Dados() with { Cnpj = "88777666000155" },
            Dados() with { Cnpj = "123" },
            Dados() with { ResponsavelTecnico = Dados().ResponsavelTecnico! with { Cpf = "123" } },
            Dados() with { Endereco = Dados().Endereco! with { Cep = "123" } },
            Dados() with { Endereco = Dados().Endereco! with { MunicipioCodigoIbge = "9999999" } }
        })
            Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync(Rota, dados)).StatusCode);
        var original = await cliente.GetFromJsonAsync<DadosContabilidadeDto>(Rota);
        Assert.Equal("99888777000166", original!.Cnpj);
        Assert.Null(original.ResponsavelTecnico);
        Assert.Null(original.Endereco);
    }

    [Fact]
    public async Task Atualizar_SemResponsavelTecnico_PreservaCadastroExistente()
    {
        await factory.PrepararTenantAsync();
        await PrepararMunicipioAsync();
        using var cliente = await LoginAsync();
        Assert.Equal(HttpStatusCode.OK, (await cliente.PutAsJsonAsync(Rota, Dados())).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.PutAsJsonAsync(Rota, Dados() with { ResponsavelTecnico = null })).StatusCode);
        Assert.NotNull((await cliente.GetFromJsonAsync<DadosContabilidadeDto>(Rota))!.ResponsavelTecnico);
    }

    private async Task PrepararMunicipioAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var estado = Estado.Criar("35", "SP", "São Paulo");
        db.Estados.Add(estado);
        db.Municipios.Add(Municipio.Criar(estado.Id, "3550308", "São Paulo"));
        await db.SaveChangesAsync();
    }

    private async Task<HttpClient> LoginAsync(string email = "admin@integracao.test", string senha = "Tenant@123")
    {
        var cliente = factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new { email, senha });
        resposta.EnsureSuccessStatusCode();
        var token = (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    private static DadosContabilidadeDto Dados() => new("Contabilidade Atualizada", "Escritório", "11.222.333/0001-44",
        "SP-123", "contato@escritorio.test", "11999998888",
        new(null, "Rua do Escritório", "10", "Sala 2", "Bairro", "São Paulo", "SP", "3550308"),
        new("Responsável", "123.456.789-01", "Contador", "SP-456",
            new(null, "Rua Residencial", "20", null, "Bairro", "São Paulo", "SP", "3550308")));
}
