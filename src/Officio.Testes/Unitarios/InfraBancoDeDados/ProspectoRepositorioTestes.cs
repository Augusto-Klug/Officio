using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ProspectoRepositorioTestes
{
    [Fact]
    public async Task ExisteComCnpjAsync_IgnoraPontuacaoDeRegistroLegado()
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
        var repositorio = new ProspectoRepositorio(db);

        var existe = await repositorio.ExisteComCnpjAsync("99093538000155");

        Assert.True(existe);
    }

    [Fact]
    public async Task ObterPagina_ProjetaResponsavelOpcionalEPreservaFiltroTenant()
    {
        var tenant = Guid.NewGuid();
        var outroTenant = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenant);
        using var db = new ContextoAplicacao(new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options,
            empresaAtual, new PublicadorEventosFake());
        var responsavel = Usuario.Create(tenant, "Maria", "maria@test.local", "hash", "Admin");
        var outroResponsavel = Usuario.Create(outroTenant, "Outro", "outro@test.local", "hash", "Admin");
        var comResponsavel = Prospecto.Create(tenant, "Com responsavel", null, null);
        var semResponsavel = Prospecto.Create(tenant, "Sem responsavel", null, null);
        var externo = Prospecto.Create(outroTenant, "Outro tenant", null, null);
        db.Users.AddRange(responsavel, outroResponsavel);
        db.Prospects.AddRange(comResponsavel, semResponsavel, externo);
        db.Entry(comResponsavel).Property(p => p.UsuarioResponsavelId).CurrentValue = responsavel.Id;
        db.Entry(externo).Property(p => p.UsuarioResponsavelId).CurrentValue = outroResponsavel.Id;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var repositorio = new ProspectoRepositorio(db);
        var consulta = repositorio.MontarConsulta(rastreamento: false);
        var pagina = repositorio.ObterPagina(consulta, 1, 20);

        Assert.Equal(2, await repositorio.ContarAsync(consulta));
        Assert.Equal(2, pagina.Count);
        Assert.Equal("Maria", Assert.Single(pagina, p => p.Id == comResponsavel.Id).ResponsavelNome);
        Assert.Null(Assert.Single(pagina, p => p.Id == semResponsavel.Id).ResponsavelNome);
        Assert.DoesNotContain(pagina, p => p.Id == externo.Id);

        empresaAtual.SetTenant(outroTenant);
        var outraPagina = repositorio.ObterPagina(repositorio.MontarConsulta(false), 1, 20);
        Assert.Equal(externo.Id, Assert.Single(outraPagina).Id);
        Assert.Equal("Outro", outraPagina[0].ResponsavelNome);
    }
}
