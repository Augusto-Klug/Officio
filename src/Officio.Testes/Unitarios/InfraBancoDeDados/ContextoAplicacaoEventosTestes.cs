using MediatR;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ContextoAplicacaoEventosTestes
{
    [Fact]
    public async Task SaveChangesAsync_PublicaEventoDeDominioUmaVez()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();

        prospecto.MarkAsConverted(Guid.NewGuid());

        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoProspectoConvertido>());
        Assert.Equal(prospecto.Id, evento.ProspectId);
    }

    [Fact]
    public async Task SaveChangesAsync_LimpaEventosAposPublicacao()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();

        prospecto.MarkAsConverted(Guid.NewGuid());
        await contexto.SaveChangesAsync();

        Assert.Empty(prospecto.EventosDominio);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId, IPublisher publicador)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, empresaAtual, publicador);
    }
}
