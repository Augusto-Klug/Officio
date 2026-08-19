using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class OrcamentoHandlersTestes
{
    [Fact]
    public async Task CriarOrcamento_ComItemDeCatalogo_CriaSnapshot()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var itemCatalogo = ItemCatalogoOrcamento.Criar(
            tenantId,
            "Escrituracao contabil",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            500m);
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.Add(itemCatalogo);
        await contexto.SaveChangesAsync();
        var handler = CriarHandler(contexto, tenantId);

        var resultado = await handler.Handle(new CriarOrcamentoComando(
            prospecto.Id,
            "Proposta mensal",
            10,
            "Observacao",
            [new CriarItemOrcamentoDto(itemCatalogo.Id, null, null, null, 2, 999m, 1)]), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        var item = await contexto.BudgetItems.SingleAsync();
        Assert.Equal(itemCatalogo.Id, item.ItemCatalogoOrcamentoId);
        Assert.Equal(itemCatalogo.Descricao, item.Descricao);
        Assert.Equal(itemCatalogo.Secao, item.Secao);
        Assert.Equal(itemCatalogo.Tipo, item.Tipo);
        Assert.Equal(itemCatalogo.ValorReferencia, item.ValorUnitario);
        Assert.Equal(1000m, item.ValorTotal);
    }

    [Fact]
    public async Task CriarOrcamento_ComItemCatalogoInativo_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var itemCatalogo = ItemCatalogoOrcamento.Criar(
            tenantId,
            "Folha de pagamento",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            150m);
        itemCatalogo.Desativar();
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.Add(itemCatalogo);
        await contexto.SaveChangesAsync();
        var handler = CriarHandler(contexto, tenantId);

        var resultado = await handler.Handle(new CriarOrcamentoComando(
            prospecto.Id,
            "Proposta mensal",
            10,
            null,
            [new CriarItemOrcamentoDto(itemCatalogo.Id, null, null, null, 1, 150m, 1)]), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("ativo", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(contexto.Budgets);
    }

    [Fact]
    public async Task AtualizarOrcamento_QuandoAceito_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        var handler = new AtualizarOrcamentoHandler(new OrcamentoRepositorio(contexto), CriarEmpresaAtual(tenantId));

        var resultado = await handler.Handle(new AtualizarOrcamentoComando(
            orcamento.Id,
            "Proposta atualizada",
            15,
            "Observacao",
            []), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("editado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ObterPorProspecto_RetornaSomenteOrcamentosDoProspecto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa A", "11222333000144", null);
        var outroProspecto = Prospecto.Create(tenantId, "Empresa B", "55666777000188", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta A", 10, null);
        var outroOrcamento = Orcamento.Criar(tenantId, outroProspecto.Id, "Proposta B", 15, null);
        contexto.Prospects.AddRange(prospecto, outroProspecto);
        contexto.Budgets.AddRange(orcamento, outroOrcamento);
        await contexto.SaveChangesAsync();
        var handler = new ObterOrcamentosPorProspectoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(
            new ObterOrcamentosPorProspectoConsulta(prospecto.Id),
            CancellationToken.None);

        var dto = Assert.Single(resultado.Valor!);
        Assert.Equal(orcamento.Id, dto.Id);
        Assert.Equal(prospecto.Id, dto.ProspectoId);
    }

    private static CriarOrcamentoHandler CriarHandler(ContextoAplicacao contexto, Guid tenantId)
        => new(
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }
}
