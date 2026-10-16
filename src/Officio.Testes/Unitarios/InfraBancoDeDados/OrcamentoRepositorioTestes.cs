using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class OrcamentoRepositorioTestes
{
    [Fact]
    public async Task ObterCatalogoAsync_FiltraInativosEIncluiQuandoSolicitado()
    {
        var tenantIdAtual = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantIdAtual);
        var itemAtivo = ItemCatalogoOrcamento.Criar(
            tenantIdAtual,
            "Escrituracao contabil",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            500m);
        var itemInativo = ItemCatalogoOrcamento.Criar(
            tenantIdAtual,
            "Folha de pagamento",
            SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.ItemMonetario,
            150m);
        itemInativo.Desativar();
        var itemOutroTenant = ItemCatalogoOrcamento.Criar(
            Guid.NewGuid(),
            "Consultoria",
            SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.ItemMonetario,
            300m);
        contexto.BudgetCatalogItems.AddRange(itemAtivo, itemInativo, itemOutroTenant);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new OrcamentoRepositorio(contexto);

        var resultado = await repositorio.ObterCatalogoAsync(false);

        var item = Assert.Single(resultado);
        Assert.Equal(itemAtivo.Id, item.Id);

        var incluindoInativos = await repositorio.ObterCatalogoAsync(true);
        Assert.Equal(2, incluindoInativos.Count);
        Assert.Contains(incluindoInativos, item => item.Id == itemInativo.Id && !item.Ativo);
    }

    [Fact]
    public async Task ObterPorProspectoAsync_RetornaOrcamentosComItens()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var item = ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Escrituracao contabil",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1);
        orcamento.AdicionarItem(item);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new OrcamentoRepositorio(contexto);

        var resultado = await repositorio.ObterPorProspectoAsync(prospecto.Id);

        var orcamentoRetornado = Assert.Single(resultado);
        Assert.Equal(orcamento.Id, orcamentoRetornado.Id);
        Assert.Collection(
            orcamentoRetornado.Itens,
            itemRetornado => Assert.Equal(item.Id, itemRetornado.Id));
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
