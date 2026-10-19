using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Servicos.Modulos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class ProspectoServicoCnpjTestes
{
    [Fact]
    public async Task CriarEConverterParaClienteAsync_PreservaDadosCadastraisNaEmpresa()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());

        var servicos = new ServiceCollection();
        var publicador = new PublicadorAtualizacaoFake();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<IPublicadorAtualizacaoTempoReal>(publicador);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(new EnderecoCepLocalidade(
                "89000000", "Rua Central", null, "Centro", "Blumenau", "SC", "4202404"))));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        servicos.AddSingleton<IConsultaMunicipioIbge>(new ConsultaMunicipioIbgeFake());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var criacao = await servico.CriarAsync(new CriarProspectoRequisicao(
            "Empresa Completa Ltda.",
            "99.093.538/0001-55",
            RegimeTributario.SimplesNacional,
            null,
            NomeFantasia: "Empresa Completa",
            PorteEmpresa: "Pequeno porte",
            CnaePrincipal: "6920601",
            DataAbertura: new DateTime(2020, 1, 10),
            Email: "contato@empresa.local",
            Telefone: "4733334444",
            Whatsapp: "47999998888",
            NomeContato: "Responsavel Comercial",
            Cep: "89000000",
            Logradouro: "Rua Central",
            Numero: "100",
            Bairro: "Centro",
            Cidade: "Blumenau",
            Uf: "sc",
            Origem: "Indicacao",
            InscricaoEstadual: "123456789",
            InscricaoMunicipal: "987654",
            TipoEmpresa: "Sociedade limitada"));

        Assert.True(criacao.Sucesso, criacao.Erro);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Prospecto, criacao.Valor!.Id), publicador.Eventos);
        var prospecto = await db.Prospects.SingleAsync();
        Assert.Equal("Empresa Completa", prospecto.NomeFantasia);
        Assert.Equal("SC", prospecto.Uf);
        Assert.Equal("123456789", prospecto.InscricaoEstadual);

        var conversao = await servico.ConverterParaClienteAsync(prospecto.Id);

        Assert.True(conversao.Sucesso, conversao.Erro);
        var empresa = await db.Empresas.SingleAsync();
        Assert.Equal("Empresa Completa", empresa.NomeFantasia);
        Assert.Equal("SimplesNacional", empresa.RegimeTributario);
        Assert.Equal("123456789", empresa.InscricaoEstadual);
        Assert.Equal("987654", empresa.InscricaoMunicipal);
        Assert.Equal("Sociedade limitada", empresa.TipoEmpresa);
        Assert.Equal("4733334444", empresa.Telefone);
        Assert.Equal("contato@empresa.local", empresa.Email);
        var cliente = await db.Clients.SingleAsync();
        var endereco = await db.Enderecos.SingleAsync();
        Assert.Equal(endereco.Id, empresa.EnderecoId);
        Assert.Equal(endereco.Id, cliente.EnderecoId);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Prospecto, prospecto.Id), publicador.Eventos);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Cliente, conversao.Valor), publicador.Eventos);
    }

    [Fact]
    public async Task AtualizarAsync_AposPersistir_PublicaAtualizacaoDoProspecto()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(tenant, "Nome original", "99093538000155", null);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();
        var publicador = new PublicadorAtualizacaoFake();
        await using var provedor = CriarServico(db, empresaAtual, publicador);
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.AtualizarAsync(
            prospecto.Id,
            new CriarProspectoRequisicao("Nome atualizado", "99093538000155", null, null));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Prospecto, prospecto.Id),
            Assert.Single(publicador.Eventos));
    }

    [Fact]
    public async Task ExcluirAsync_AposPersistir_PublicaAtualizacaoDoProspecto()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(tenant, "Nome para excluir", "99093538000155", null);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();
        var publicador = new PublicadorAtualizacaoFake();
        await using var provedor = CriarServico(db, empresaAtual, publicador);
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ExcluirAsync(prospecto.Id);

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Empty(await db.Prospects.ToListAsync());
        Assert.Equal(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Prospecto, prospecto.Id),
            Assert.Single(publicador.Eventos));
    }

    [Fact]
    public async Task CriarAsync_ComCepInexistente_RetornaFalhaSemPersistirProspecto()
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(Guid.NewGuid());
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(null)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();

        var resultado = await provedor.GetRequiredService<IProspectoServico>().CriarAsync(
            new CriarProspectoRequisicao(
                "Empresa com CEP invalido",
                "99093538000155",
                RegimeTributario.SimplesNacional,
                null,
                Cep: "00000000"));

        Assert.False(resultado.Sucesso);
        Assert.Equal(LocalidadeErros.CepInvalido, resultado.Erro);
        Assert.Empty(db.Prospects);
    }

    [Fact]
    public async Task CriarAsync_ComConsultaCepIndisponivel_PermiteEnderecoManual()
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(Guid.NewGuid());
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarFalha(LocalidadeErros.ConsultaCepIndisponivel)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();

        var resultado = await provedor.GetRequiredService<IProspectoServico>().CriarAsync(
            new CriarProspectoRequisicao(
                "Empresa com endereco manual",
                "99093538000155",
                RegimeTributario.SimplesNacional,
                null,
                Cep: "89000000",
                Logradouro: "Rua informada manualmente",
                Cidade: "Blumenau",
                Uf: "SC"));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal("89000000", Assert.Single(db.Prospects).Cep);
    }

    [Fact]
    public async Task ConverterParaClienteAsync_ComCnpj_CriaEmpresaEVinculaCliente()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(
            tenant,
            "Empresa para admissao",
            "99093538000155",
            RegimeTributario.SimplesNacional);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();

        var servicos = new ServiceCollection();
        var publicador = new PublicadorAtualizacaoFake();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<IPublicadorAtualizacaoTempoReal>(publicador);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(null)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ConverterParaClienteAsync(prospecto.Id);

        Assert.True(resultado.Sucesso);
        var cliente = Assert.Single(db.Clients);
        var empresa = Assert.Single(db.Empresas);
        Assert.Equal(empresa.Id, cliente.EmpresaId);
        Assert.Equal(prospecto.Cnpj, empresa.Cnpj);
        Assert.Equal(RegimeTributario.SimplesNacional.ToString(), empresa.RegimeTributario);
        var statusCliente = Assert.Single(db.StatusClientes);
        Assert.Equal("ATIVO", statusCliente.Codigo);
        Assert.Equal(statusCliente.Id, cliente.StatusClienteId);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Prospecto, prospecto.Id), publicador.Eventos);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenant, EntidadesAtualizacaoTempoReal.Cliente, resultado.Valor), publicador.Eventos);
    }

    [Fact]
    public async Task ConverterParaClienteAsync_SemCnpj_RetornaFalhaSemCriarCliente()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(tenant, "Empresa em abertura", null, null);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(null)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ConverterParaClienteAsync(prospecto.Id);

        Assert.False(resultado.Sucesso);
        Assert.Empty(db.Clients);
        Assert.Empty(db.Empresas);
    }

    [Theory]
    [InlineData("99093538000155")]
    [InlineData("99.093.538/0001-55")]
    public async Task ObterAsync_EncontraCnpjLegadoIndependentementeDaPontuacaoDaBusca(string busca)
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(tenant, "Empresa formatada", "99093538000155", null);
        db.Prospects.Add(prospecto);
        db.Entry(prospecto).Property(p => p.Cnpj).CurrentValue = "99.093.538/0001-55";
        await db.SaveChangesAsync();

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(null)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ObterAsync(new ObterProspectosFiltro(1, 20, null, busca, null));

        Assert.True(resultado.Sucesso);
        Assert.Equal(prospecto.Id, Assert.Single(resultado.Valor!.Data).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AtualizarAsync_ComCepInvalidoOuMunicipioDivergente_NaoAlteraCadastro(bool municipioDivergente)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(Guid.NewGuid());
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(empresaAtual.TenantId, "Cadastro original", "99093538000155", null);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();
        var cep = municipioDivergente
            ? new EnderecoCepLocalidade("89010000", "Rua", null, "Centro", "Blumenau", "SC", "4202404")
            : null;
        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(Resultado<EnderecoCepLocalidade?>.CriarSucesso(cep)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();

        var resultado = await provedor.GetRequiredService<IProspectoServico>().AtualizarAsync(
            prospecto.Id, new("Nao salvar", "99093538000155", null, [], Cep: "89010000", MunicipioCodigoIbge: "4205407"));

        Assert.False(resultado.Sucesso);
        Assert.Equal(municipioDivergente ? LocalidadeErros.MunicipioDivergenteDoCep : LocalidadeErros.CepInvalido, resultado.Erro);
        Assert.Equal("Cadastro original", prospecto.RazaoSocial);
        Assert.Null(prospecto.Cep);
    }

    private sealed class ConsultaMunicipioIbgeFake : IConsultaMunicipioIbge
    {
        public Task<MunicipioIbge?> ObterPorNomeUfAsync(
            string nome,
            string uf,
            CancellationToken cancellationToken = default)
            => Task.FromResult<MunicipioIbge?>(new MunicipioIbge("4202404", "Blumenau", "SC"));
    }

    private sealed class LocalidadeServicoFake(Resultado<EnderecoCepLocalidade?> resultadoCep)
        : ILocalidadeServico
    {
        public Task<Resultado<IReadOnlyCollection<EstadoLocalidade>>> ListarEstadosAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<IReadOnlyCollection<EstadoLocalidade>>.CriarSucesso([]));

        public Task<Resultado<IReadOnlyCollection<MunicipioLocalidade>>> ListarMunicipiosPorUfAsync(
            string uf,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Resultado<IReadOnlyCollection<MunicipioLocalidade>>.CriarSucesso([]));

        public Task<Resultado<EnderecoCepLocalidade?>> ConsultarCepAsync(
            string cep,
            CancellationToken cancellationToken = default)
            => Task.FromResult(resultadoCep);
    }

    private sealed class PublicadorAtualizacaoFake : IPublicadorAtualizacaoTempoReal
    {
        public List<EventoAtualizacaoTempoReal> Eventos { get; } = [];

        public Task PublicarAsync(EventoAtualizacaoTempoReal evento, CancellationToken cancellationToken = default)
        {
            Eventos.Add(evento);
            return Task.CompletedTask;
        }
    }

    private static ServiceProvider CriarServico(
        ContextoAplicacao contexto,
        IEmpresaAtual empresaAtual,
        IPublicadorAtualizacaoTempoReal publicador)
    {
        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(contexto));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AddSingleton<IPublicadorAtualizacaoTempoReal>(publicador);
        servicos.AddSingleton<ILocalidadeServico>(new LocalidadeServicoFake(
            Resultado<EnderecoCepLocalidade?>.CriarSucesso(null)));
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        return servicos.BuildServiceProvider();
    }
}
