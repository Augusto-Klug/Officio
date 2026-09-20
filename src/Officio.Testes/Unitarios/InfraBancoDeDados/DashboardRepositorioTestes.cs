using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class DashboardRepositorioTestes
{
    [Fact]
    public async Task ObterVisaoGeralAsync_ConsideraSomenteDadosDoTenantAtual()
    {
        var banco = Guid.NewGuid().ToString();
        var tenantAtual = Guid.NewGuid();
        await using (var contextoSeed = CriarContexto(banco, tenantAtual))
        {
            AdicionarCenarioAtivo(contextoSeed, tenantAtual, "Empresa atual", 500m);
            AdicionarCenarioAtivo(contextoSeed, Guid.NewGuid(), "Outra empresa", 900m);
            await contextoSeed.SaveChangesAsync();
        }

        await using var contexto = CriarContexto(banco, tenantAtual);
        var resultado = await new DashboardRepositorio(contexto).ObterVisaoGeralAsync();

        Assert.Equal(1, resultado.ProspectosEmAberto);
        Assert.Equal(1, resultado.ClientesAtivos);
        Assert.Equal(1, resultado.ContratosAtivos);
        Assert.Equal(1, resultado.OrcamentosAguardandoRetorno);
        Assert.Equal(500m, resultado.ReceitaMensalContratada);
    }

    private static void AdicionarCenarioAtivo(
        ContextoAplicacao contexto,
        Guid tenantId,
        string razaoSocial,
        decimal valor)
    {
        var prospecto = Prospecto.Create(tenantId, razaoSocial, null, null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Mensalidade", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            valor,
            1));
        orcamento.AlterarStatus(StatusOrcamento.Enviado);
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 1, 1),
            null,
            true,
            null);
        contrato.VincularCliente(cliente.Id);
        contrato.MarkAsSigned(new DateTime(2026, 1, 2));

        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
    }

    private static ContextoAplicacao CriarContexto(string banco, Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(banco)
            .Options;
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }
}
