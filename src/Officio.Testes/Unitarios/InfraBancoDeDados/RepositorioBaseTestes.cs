using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class RepositorioBaseTestes
{
    [Fact]
    public async Task ObterQuandoAsync_SemRastreamento_RetornaEntidadesSemTracking()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        contexto.Prospects.Add(Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null));
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new RepositorioBase<ContextoAplicacao, Guid>(contexto);

        var resultado = await repositorio.ObterQuandoAsync<Prospecto>(
            p => p.Cnpj == "11222333000144",
            rastreamento: false);

        var prospecto = Assert.Single(resultado);
        Assert.Equal("Empresa Teste", prospecto.RazaoSocial);
        Assert.Empty(contexto.ChangeTracker.Entries<Prospecto>());
    }

    [Fact]
    public async Task AdicionarAsync_SalvaEntidadeNoContexto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        IRepositorioBase<ContextoAplicacao, Guid> repositorio = new RepositorioBase<ContextoAplicacao, Guid>(contexto);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);

        await repositorio.AdicionarAsync(prospecto);
        await contexto.SaveChangesAsync();

        Assert.True(await contexto.Prospects.AnyAsync(p => p.Id == prospecto.Id));
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }
}
