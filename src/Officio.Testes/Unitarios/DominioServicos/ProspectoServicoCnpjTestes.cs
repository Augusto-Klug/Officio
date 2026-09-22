using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Modulos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class ProspectoServicoCnpjTestes
{
    [Fact]
    public async Task ConverterParaClienteAsync_ComCnpj_CriaEmpresaEVinculaCliente()
    {
        var tenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        await using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var prospecto = Prospecto.Create(tenant, "Empresa para admissao", "99093538000155", null);
        db.Prospects.Add(prospecto);
        await db.SaveChangesAsync();

        var servicos = new ServiceCollection();
        servicos.AddSingleton<IProspectoRepositorio>(new ProspectoRepositorio(db));
        servicos.AddSingleton<IEmpresaAtual>(empresaAtual);
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ConverterParaClienteAsync(prospecto.Id);

        Assert.True(resultado.Sucesso);
        var cliente = Assert.Single(db.Clients);
        var empresa = Assert.Single(db.Empresas);
        Assert.Equal(empresa.Id, cliente.EmpresaId);
        Assert.Equal(prospecto.Cnpj, empresa.Cnpj);
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
        servicos.AdicionarServicos(new ConfigurationBuilder().Build());
        await using var provedor = servicos.BuildServiceProvider();
        var servico = provedor.GetRequiredService<IProspectoServico>();

        var resultado = await servico.ObterAsync(new ObterProspectosFiltro(1, 20, null, busca, null));

        Assert.True(resultado.Sucesso);
        Assert.Equal(prospecto.Id, Assert.Single(resultado.Valor!.Data).Id);
    }
}
