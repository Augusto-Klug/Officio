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
        var repositorio = new DashboardRepositorio(contexto);
        var resultado = await repositorio.ObterVisaoGeralAsync(new DateOnly(2026, 9, 1));
        var projecao = await repositorio.ListarContratosParaProjecaoAsync(
            new DateTime(2026, 9, 1),
            new DateTime(2027, 2, 28));

        Assert.Equal(1, resultado.ProspectosEmAberto);
        Assert.Equal(1, resultado.ClientesAtivos);
        Assert.Equal(1, resultado.ContratosAtivos);
        Assert.Equal(1, resultado.OrcamentosAguardandoRetorno);
        Assert.Equal(500m, resultado.FaturamentoCompetenciaAtual);
        Assert.Equal(500m, Assert.Single(projecao).ReceitaMensal);
    }

    private static void AdicionarCenarioAtivo(
        ContextoAplicacao contexto,
        Guid tenantId,
        string razaoSocial,
        decimal valor)
    {
        var empresa = Empresa.CriarConstituida(tenantId, "11222333000144", razaoSocial);
        var prospecto = Prospecto.Create(tenantId, razaoSocial, "11222333000144", null);
        prospecto.VincularEmpresa(empresa.Id);
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
            1,
            NaturezaCobrancaItemOrcamento.HonorarioMensal));
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Implantacao",
            SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.ItemMonetario,
            1,
            1000m,
            2,
            NaturezaCobrancaItemOrcamento.NaoRecorrente));
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
        var statusFaturamento = contexto.StatusFaturamentos.Local
            .FirstOrDefault(item => item.Codigo == "ABERTO")
            ?? new StatusFaturamento("ABERTO", "Aberto");
        var faturamento = FaturamentoMensal.Criar(
            tenantId,
            contrato.Id,
            statusFaturamento.Id,
            new DateOnly(2026, 9, 1),
            0,
            0,
            valor,
            0m);

        contexto.Empresas.Add(empresa);
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        if (contexto.Entry(statusFaturamento).State == EntityState.Detached)
            contexto.StatusFaturamentos.Add(statusFaturamento);
        contexto.FaturamentosMensais.Add(faturamento);
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
