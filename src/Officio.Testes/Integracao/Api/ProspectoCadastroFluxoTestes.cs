using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Testes.Integracao.Infraestrutura;

namespace Officio.Testes.Integracao.Api;

public class ProspectoCadastroFluxoTestes(OfficioWebApplicationFactory factory)
    : IClassFixture<OfficioWebApplicationFactory>
{
    [Fact]
    public async Task Inicial_EditaDadosESociosDepoisExcluiComAuditoria()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var cadastro = await ObterDadosAsync(cliente, id);
        var socio = cadastro["socios"]![0]!.AsObject();
        var socioId = socio["id"]!.GetValue<Guid>();
        cadastro["razaoSocial"] = "Empresa revisada";
        cadastro["cnpj"] = "99.093.538/0001-55";
        cadastro["regimeTributario"] = 2;
        cadastro["email"] = "novo@empresa.test";
        cadastro["telefone"] = "47999998888";
        cadastro["inscricaoEstadual"] = "12345";
        socio["nome"] = "Socio revisado";
        socio["participacao"] = 70;
        cadastro["socios"]!.AsArray().Add(new JsonObject { ["nome"] = "Novo socio", ["participacao"] = 30 });

        var atualizacao = await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", cadastro);
        Assert.Equal(HttpStatusCode.NoContent, atualizacao.StatusCode);
        var revisado = await ObterDadosAsync(cliente, id);
        Assert.Equal("Empresa revisada", revisado["razaoSocial"]!.GetValue<string>());
        Assert.Equal("99093538000155", revisado["cnpj"]!.GetValue<string>());
        Assert.Equal(2, revisado["regimeTributario"]!.GetValue<int>());
        Assert.Equal("novo@empresa.test", revisado["email"]!.GetValue<string>());
        Assert.Equal("12345", revisado["inscricaoEstadual"]!.GetValue<string>());
        Assert.Equal(2, revisado["socios"]!.AsArray().Count);
        Assert.Contains(revisado["socios"]!.AsArray(), s => s!["id"]!.GetValue<Guid>() == socioId);

        revisado["socios"] = new JsonArray();
        Assert.Equal(HttpStatusCode.NoContent,
            (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", revisado)).StatusCode);
        Assert.Empty((await ObterDadosAsync(cliente, id))["socios"]!.AsArray());

        var exclusao = await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}");
        Assert.Equal(HttpStatusCode.NoContent, exclusao.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        Assert.Empty(await db.Socios.IgnoreQueryFilters().Where(s => s.ProspectId == id).ToListAsync());
        var auditorias = await db.AuditLogs.IgnoreQueryFilters().Select(a => a.Acao).ToListAsync();
        Assert.Contains("AtualizarProspectoComando:ALTERADO", auditorias);
        Assert.Contains("ExcluirProspectoComando:REMOVIDO", auditorias);
    }

    [Theory]
    [InlineData(StatusProspecto.PropostaEnviada)]
    [InlineData(StatusProspecto.Convertido)]
    [InlineData(StatusProspecto.Perdido)]
    [InlineData(StatusProspecto.Inativo)]
    public async Task ForaDaEtapaInicial_RejeitaEditarEExcluir(StatusProspecto status)
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        dados["razaoSocial"] = "Nao pode alterar";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            (await db.Prospects.IgnoreQueryFilters().SingleAsync(p => p.Id == id)).UpdateStatus(status);
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Conflict, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal("Empresa inicial", (await ObterDadosAsync(cliente, id))["razaoSocial"]!.GetValue<string>());
    }

    [Fact]
    public async Task OutroParceiro_NaoObtemEditaOuExcluiCadastro()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        await factory.AdicionarTenantAsync("88777666000155", "outro@integracao.test", "Outro@123");
        using var outro = await ClienteAutenticadoAsync("outro@integracao.test", "Outro@123");

        Assert.Equal(HttpStatusCode.NotFound, (await outro.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await outro.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
    }

    [Fact]
    public async Task Editar_RejeitaCnpjDuplicadoCamposInvalidosESocioDeOutroCadastro()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var primeiro = await CriarAsync(cliente);
        var segundo = await CriarAsync(cliente, "11222333000144");
        var dados = await ObterDadosAsync(cliente, primeiro);
        dados["cnpj"] = "11.222.333/0001-44";
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        dados["cnpj"] = "12";
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        dados["cnpj"] = "99093538000155";
        dados["socios"] = (await ObterDadosAsync(cliente, segundo))["socios"]!.DeepClone();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{primeiro}", dados)).StatusCode);
        Assert.Equal("99093538000155", (await ObterDadosAsync(cliente, primeiro))["cnpj"]!.GetValue<string>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exclusao_ComVinculos_RejeitaSemApagarHistorico(bool documento)
    {
        var tenant = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            if (documento)
                db.Documents.Add(Documento.Create(tenant, "arquivo.pdf", TipoDocumento.CartaoCnpj, "storage/arquivo.pdf", 1m, prospectId: id));
            else
                db.Budgets.Add(Orcamento.Criar(tenant, id, "Orcamento vinculado", 10, null));
            await db.SaveChangesAsync();
        }

        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}")).StatusCode);
    }

    [Fact]
    public async Task RemoverSocio_ComDocumentoVinculado_RejeitaSemAlterarCadastro()
    {
        var tenant = await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        var socioId = dados["socios"]![0]!["id"]!.GetValue<Guid>();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            db.Documents.Add(Documento.Create(tenant, "identidade.pdf", TipoDocumento.RgSocio, "storage/rg.pdf", 1m, socioId: socioId));
            await db.SaveChangesAsync();
        }
        dados["razaoSocial"] = "Nao deve salvar";
        dados["socios"] = new JsonArray();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.DeleteAsync($"/api/v1/prospectos/Excluir/{id}")).StatusCode);
        var atual = await ObterDadosAsync(cliente, id);
        Assert.Equal("Empresa inicial", atual["razaoSocial"]!.GetValue<string>());
        Assert.Single(atual["socios"]!.AsArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CadastroComercial_PermiteSemSociosOuSocioTotalmenteIncompleto(bool adicionarSocio)
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var resposta = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Cadastro opcional",
            socios = adicionarSocio ? new[] { new Dictionary<string, object?>() } : []
        });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var id = (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<Guid>();
        var dados = await ObterDadosAsync(cliente, id);
        Assert.Equal(adicionarSocio ? 1 : 0, dados["socios"]!.AsArray().Count);
        if (adicionarSocio)
        {
            var socio = dados["socios"]![0]!;
            Assert.NotEqual(Guid.Empty, socio["pessoaId"]!.GetValue<Guid>());
            Assert.Null(socio["participacao"]);
            Assert.Null(socio["tipoRelacaoId"]);
            Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
            dados["socios"] = new JsonArray();
            Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
            Assert.Empty((await ObterDadosAsync(cliente, id))["socios"]!.AsArray());
        }
    }

    [Theory]
    [InlineData("SOCIO", "Sócio")]
    [InlineData("SOCIO_ADMINISTRADOR", "Sócio administrador")]
    public async Task Socio_PreservaPessoaProfissaoEnderecoETipoNaEdicaoEConversao(string codigo, string descricao)
    {
        await factory.PrepararTenantAsync();
        var tipo = new TipoRelacao(codigo, descricao, "SOCIETARIO");
        var estado = Estado.Criar("42", "SC", "Santa Catarina");
        var municipio = Municipio.Criar(estado.Id, "4202404", "Blumenau");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            db.TiposRelacao.Add(tipo); db.Estados.Add(estado); db.Municipios.Add(municipio);
            await db.SaveChangesAsync();
        }
        using var cliente = await ClienteAutenticadoAsync();
        var tipos = await cliente.GetFromJsonAsync<JsonArray>("/api/v1/prospectos/TiposRelacaoSocietaria");
        Assert.Contains(tipos!, t => t!["id"]!.GetValue<Guid>() == tipo.Id);
        var resposta = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Cadastro societario", cnpj = "99093538000155",
            socios = new[] { new { nome = "Ana", cpf = "117.892.999-00", profissao = "Engenheira",
                participacao = 40, tipoRelacaoId = tipo.Id, endereco = new { logradouro = "Rua inicial" } } }
        });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var id = (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<Guid>();
        var dados = await ObterDadosAsync(cliente, id);
        var socio = dados["socios"]![0]!;
        var pessoaId = socio["pessoaId"]!.GetValue<Guid>();
        Assert.Equal("Rua inicial", socio["endereco"]!["logradouro"]!.GetValue<string>());
        Assert.Null(socio["endereco"]!["municipioCodigoIbge"]);
        socio["nome"] = "Ana revisada"; socio["profissao"] = "Arquiteta";
        socio["participacao"] = null;
        socio["endereco"] = new JsonObject { ["logradouro"] = "Rua final", ["numero"] = "10", ["complemento"] = "Sala 2",
            ["bairro"] = "Centro", ["uf"] = "SC", ["municipioCodigoIbge"] = "4202404" };
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        var editado = (await ObterDadosAsync(cliente, id))["socios"]![0]!;
        Assert.Equal(pessoaId, editado["pessoaId"]!.GetValue<Guid>());
        Assert.Equal("Arquiteta", editado["profissao"]!.GetValue<string>());
        Assert.Equal("SC", editado["endereco"]!["uf"]!.GetValue<string>());
        Assert.Equal("4202404", editado["endereco"]!["municipioCodigoIbge"]!.GetValue<string>());
        Assert.Equal(HttpStatusCode.Created, (await cliente.PostAsync($"/api/v1/prospectos/ConverterParaCliente/{id}", null)).StatusCode);
        using var verificacao = factory.Services.CreateScope();
        var contexto = verificacao.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var convertido = await contexto.Clients.IgnoreQueryFilters().Include(c => c.Socios).ThenInclude(s => s.Pessoa)
            .ThenInclude(p => p.Endereco).SingleAsync(c => c.ProspectOrigemId == id);
        var socioCliente = Assert.Single(convertido.Socios);
        Assert.Equal(pessoaId, socioCliente.PessoaId);
        Assert.Equal(tipo.Id, socioCliente.TipoRelacaoId);
        Assert.Equal("Ana revisada", socioCliente.Nome);
        Assert.Equal("11789299900", socioCliente.Cpf);
        Assert.Equal("Arquiteta", socioCliente.Pessoa.Profissao);
        Assert.Equal(municipio.Id, socioCliente.Pessoa.Endereco!.MunicipioId);
        Assert.Equal("Rua final", socioCliente.Pessoa.Endereco.Logradouro);
        Assert.Equal("Sala 2", socioCliente.Pessoa.Endereco.Complemento);
        Assert.Null(socioCliente.Participacao);
        Assert.Single(await contexto.Set<Pessoa>().ToListAsync());
        Assert.Single(await contexto.Set<Endereco>().ToListAsync());
    }

    [Fact]
    public async Task Socio_RejeitaLocalidadeInconsistenteETipoInvalidoSemPersistirAlteracao()
    {
        await factory.PrepararTenantAsync();
        using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        var dados = await ObterDadosAsync(cliente, id);
        dados["socios"]![0]!["tipoRelacaoId"] = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        dados["socios"]![0]!["tipoRelacaoId"] = null;
        dados["socios"]![0]!["endereco"] = new JsonObject { ["municipioCodigoIbge"] = "9999999", ["uf"] = "SC" };
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Null((await ObterDadosAsync(cliente, id))["socios"]![0]!["endereco"]);
    }

    [Fact]
    public async Task Representante_CompletaSociosAposPropostaSemAlterarEmpresaERecaregaLista()
    {
        var tenantId = await factory.PrepararTenantAsync(); using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        Guid orcamentoId;
        var estado = Estado.Criar("42", "SC", "Santa Catarina");
        var municipio = Municipio.Criar(estado.Id, "4202404", "Blumenau");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var p = await db.Prospects.IgnoreQueryFilters().SingleAsync(p => p.Id == id);
            p.UpdateStatus(StatusProspecto.PropostaEnviada);
            p.VincularMunicipio(municipio.Id, "Blumenau", "SC");
            p.AtualizarDadosCadastrais(null, null, null, null, null, null, null, null, null,
                "89010000", "Rua da CONTRATANTE", "10", null, "Centro", "Blumenau", "SC", null, null, null, null, null);
            var orcamento = Orcamento.Criar(p.TenantId, p.Id, "Aceito", 10, null);
            orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(p.TenantId, orcamento.Id, null, "Honorário mensal",
                SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1,
                NaturezaCobrancaItemOrcamento.HonorarioMensal));
            orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(p.TenantId, orcamento.Id, null, "Preço por folha",
                SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 40, 12m, 2,
                NaturezaCobrancaItemOrcamento.PorFolha));
            orcamento.Aceitar();
            db.Budgets.Add(orcamento); orcamentoId = orcamento.Id;
            db.Estados.Add(estado); db.Municipios.Add(municipio); await db.SaveChangesAsync();

            var parceiro = await db.Parceiros.IgnoreQueryFilters().SingleAsync(parceiro => parceiro.Id == tenantId);
            parceiro.AtualizarDadosContabilidade(parceiro.Cnpj, parceiro.RazaoSocial, null, null, null, "CRC-SC-12345");
            var enderecoContratada = Endereco.Criar(municipio.Id, "Rua da Contabilidade", "89010000", "10", "Centro");
            var enderecoResponsavel = Endereco.Criar(municipio.Id, "Rua do Responsável", "89010000", "20", "Centro");
            db.Enderecos.AddRange(enderecoContratada, enderecoResponsavel);
            parceiro.VincularEndereco(enderecoContratada.Id);
            var responsavel = ResponsavelTecnico.Criar(tenantId, "Responsável Técnico", "12345678901", "Contador", "CRC-SC-54321");
            responsavel.VincularEndereco(enderecoResponsavel.Id);
            db.ResponsaveisTecnicos.Add(responsavel);
            await db.SaveChangesAsync();
        }
        var lista = await cliente.GetFromJsonAsync<JsonObject>($"/api/v1/orcamentos/ObterRepresentantes/{orcamentoId}");
        var candidato = lista!["socios"]![0]!;
        var socioId = candidato["id"]!.GetValue<Guid>();
        Assert.False(candidato["cadastroCompleto"]!.GetValue<bool>());
        var respostaIncompleta = await cliente.PostAsJsonAsync($"/api/v1/orcamentos/ConverterParaContrato/{orcamentoId}",
            new { dataInicio = DateTime.Today, representanteSocioId = socioId });
        Assert.Equal(HttpStatusCode.BadRequest, respostaIncompleta.StatusCode);
        var pendencias = await respostaIncompleta.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Contains("Não foi possível gerar o contrato", pendencias!["error"]!.GetValue<string>());
        Assert.NotEmpty(pendencias["errors"]!["pendencias"]!.AsArray());
        var dados = await ObterDadosAsync(cliente, id);
        dados["razaoSocial"] = "Nao alterar empresa";
        dados["socios"]![0]!["profissao"] = "Arquiteta";
        dados["socios"]![0]!["endereco"] = new JsonObject { ["logradouro"] = "Rua", ["numero"] = "10",
            ["bairro"] = "Centro", ["uf"] = "SC", ["municipioCodigoIbge"] = "4202404" };
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/Atualizar/{id}", dados)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/AtualizarSocios/{id}", dados)).StatusCode);
        Assert.Equal("Empresa inicial", (await ObterDadosAsync(cliente, id))["razaoSocial"]!.GetValue<string>());
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var s = await db.Socios.IgnoreQueryFilters().Include(s => s.Pessoa).ThenInclude(p => p.Endereco).SingleAsync(s => s.Id == socioId);
            var tipo = new TipoRelacao("SOCIO", "Sócio", "SOCIETARIO"); db.TiposRelacao.Add(tipo);
            s.AtualizarVinculo(null, tipo);
            s.Pessoa.Endereco!.AtualizarParcial(municipio.Id, estado.Id, "Rua", "89010000", "10", "Centro", null);
            await db.SaveChangesAsync();
        }
        lista = await cliente.GetFromJsonAsync<JsonObject>($"/api/v1/orcamentos/ObterRepresentantes/{orcamentoId}");
        Assert.True(lista!["socios"]![0]!["cadastroCompleto"]!.GetValue<bool>());
        var resposta = await cliente.PostAsJsonAsync($"/api/v1/orcamentos/ConverterParaContrato/{orcamentoId}",
            new { dataInicio = DateTime.Today, duracaoAnos = 4,
                competenciaInicialHonorarios = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1),
                representanteSocioId = socioId });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var contratoId = await resposta.Content.ReadFromJsonAsync<Guid>();
        var contrato = await cliente.GetFromJsonAsync<JsonObject>($"/api/v1/contratos/ObterPorId/{contratoId}");
        Assert.Equal(socioId, contrato!["representanteSocioId"]!.GetValue<Guid>());
        dados = await ObterDadosAsync(cliente, id); dados["socios"] = new JsonArray();
        Assert.Equal(HttpStatusCode.BadRequest, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/AtualizarSocios/{id}", dados)).StatusCode);
    }

    [Fact]
    public async Task CadastroSocios_PermiteCompletarVinculoExistenteSomenteNoClienteCorrespondente()
    {
        await factory.PrepararTenantAsync(); using var cliente = await ClienteAutenticadoAsync();
        var id = await CriarAsync(cliente);
        Assert.Equal(HttpStatusCode.Created, (await cliente.PostAsync($"/api/v1/prospectos/ConverterParaCliente/{id}", null)).StatusCode);
        Guid socioClienteId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacao>();
            var original = await db.Socios.IgnoreQueryFilters().SingleAsync(s => s.ProspectId == id);
            socioClienteId = (await db.Socios.IgnoreQueryFilters().SingleAsync(s => s.ClientId.HasValue)).Id;
            db.Socios.Remove(original); await db.SaveChangesAsync();
        }
        var resposta = await cliente.GetFromJsonAsync<JsonObject>($"/api/v1/prospectos/ObterPorId/{id}?incluirSociosCliente=true");
        var dados = resposta!["dados"]!.AsObject();
        Assert.Equal(socioClienteId, dados["socios"]![0]!["id"]!.GetValue<Guid>());
        dados["socios"]![0]!["profissao"] = "Arquiteta";
        Assert.Equal(HttpStatusCode.NoContent, (await cliente.PutAsJsonAsync($"/api/v1/prospectos/AtualizarSocios/{id}", dados)).StatusCode);
        using var verificacao = factory.Services.CreateScope();
        var contexto = verificacao.ServiceProvider.GetRequiredService<ContextoAplicacao>();
        var socio = await contexto.Socios.IgnoreQueryFilters().Include(s => s.Pessoa).SingleAsync(s => s.Id == socioClienteId);
        Assert.Equal("Arquiteta", socio.Pessoa.Profissao);
        Assert.Single(await contexto.Set<Pessoa>().ToListAsync());
    }

    private static async Task<Guid> CriarAsync(HttpClient cliente, string cnpj = "99093538000155")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/v1/prospectos/Criar", new
        {
            razaoSocial = "Empresa inicial", cnpj, regimeTributario = 0,
            socios = new[] { new { nome = "Socio inicial", cpf = "11789299900", participacao = 100 } }
        });
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<Guid>();
    }

    private static async Task<JsonObject> ObterDadosAsync(HttpClient cliente, Guid id)
    {
        var resposta = await cliente.GetAsync($"/api/v1/prospectos/ObterPorId/{id}");
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<JsonObject>())!["dados"]!.AsObject();
    }

    private async Task<HttpClient> ClienteAutenticadoAsync(string email = "admin@integracao.test", string senha = "Tenant@123")
    {
        var cliente = factory.CreateClient();
        var login = await cliente.PostAsJsonAsync("/api/v1/autenticacao/Login", new { email, senha });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = (await login.Content.ReadFromJsonAsync<JsonObject>())!["token"]!.GetValue<string>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
