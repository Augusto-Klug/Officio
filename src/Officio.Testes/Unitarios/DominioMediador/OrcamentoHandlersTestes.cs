using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentoPorId;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.Dominio.Servicos.Servicos;
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
        Assert.Equal(999m, item.ValorUnitario);
        Assert.Equal(1998m, item.ValorTotal);
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
    public async Task CriarOrcamento_ComPrecoNegativo_Falha()
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
            null,
            [new CriarItemOrcamentoDto(itemCatalogo.Id, null, null, null, 1, -1m, 1)]), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("negativo", resultado.Erro, StringComparison.OrdinalIgnoreCase);
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
        var handler = new AtualizarOrcamentoHandler(CriarServico(contexto, tenantId));

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
    public async Task AtualizarOrcamento_ComItemCatalogoExistente_PreservaMetadadosEAtualizaPreco()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var itemCatalogo = ItemCatalogoOrcamento.Criar(
            tenantId,
            "Descricao atual do catalogo",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            500m);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var itemHistorico = ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            itemCatalogo.Id,
            "Descricao historica",
            SecaoItemOrcamento.ServicosInclusos,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1);
        orcamento.AdicionarItem(itemHistorico);
        itemCatalogo.Desativar();
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.Add(itemCatalogo);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var handler = new AtualizarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new AtualizarOrcamentoComando(
            orcamento.Id,
            "Proposta atualizada",
            15,
            null,
            [new CriarItemOrcamentoDto(
                itemCatalogo.Id,
                "Descricao ajustada",
                SecaoItemOrcamento.ServicosSeparados,
                TipoItemOrcamento.ItemMonetario,
                2,
                750m,
                1,
                NaturezaCobrancaItemOrcamento.HonorarioMensal,
                itemHistorico.Id)]), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();
        var itemAtualizado = await contexto.BudgetItems.SingleAsync();
        Assert.Equal("Descricao historica", itemAtualizado.Descricao);
        Assert.Equal(SecaoItemOrcamento.ServicosInclusos, itemAtualizado.Secao);
        Assert.Equal(TipoItemOrcamento.ItemMonetario, itemAtualizado.Tipo);
        Assert.Equal(750m, itemAtualizado.ValorUnitario);
        Assert.Equal(1500m, itemAtualizado.ValorTotal);
        Assert.Equal(2, itemAtualizado.Quantidade);
    }

    [Fact]
    public async Task AtualizarOrcamento_ComPrecoNegativo_FalhaSemAlterarItem()
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
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var item = ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            itemCatalogo.Id,
            itemCatalogo.Descricao,
            itemCatalogo.Secao,
            itemCatalogo.Tipo,
            1,
            500m,
            1);
        orcamento.AdicionarItem(item);
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.Add(itemCatalogo);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var handler = new AtualizarOrcamentoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new AtualizarOrcamentoComando(
            orcamento.Id,
            "Proposta mensal",
            10,
            null,
            [new CriarItemOrcamentoDto(itemCatalogo.Id, null, null, null, 1, -1m, 1, null, item.Id)]), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("negativo", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        contexto.ChangeTracker.Clear();
        var itemPersistido = await contexto.BudgetItems.SingleAsync();
        Assert.Equal(500m, itemPersistido.ValorUnitario);
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
        var handler = new ObterOrcamentosPorProspectoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new ObterOrcamentosPorProspectoConsulta(prospecto.Id),
            CancellationToken.None);

        var dto = Assert.Single(resultado.Valor!);
        Assert.Equal(orcamento.Id, dto.Id);
        Assert.Equal(prospecto.Id, dto.ProspectoId);
    }

    [Fact]
    public async Task ObterPorId_RetornaOrcamentoComItens()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa A", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta A", 10, "Observacao");
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var handler = new ObterOrcamentoPorIdHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new ObterOrcamentoPorIdConsulta(orcamento.Id), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(orcamento.Id, resultado.Valor!.Id);
        Assert.Equal("Proposta A", resultado.Valor.Titulo);
        var item = Assert.Single(resultado.Valor.Itens);
        Assert.Equal("Honorarios", item.Descricao);
        Assert.Equal(500m, item.ValorTotal);
    }

    [Fact]
    public async Task ObterPorId_QuandoInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var handler = new ObterOrcamentoPorIdHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new ObterOrcamentoPorIdConsulta(Guid.NewGuid()), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Orcamento nao encontrado", resultado.Erro);
    }

    [Fact]
    public async Task ObterPorId_QuandoOrcamentoDeOutroTenant_Falha()
    {
        var tenantId = Guid.NewGuid();
        var tenantEstrangeiroId = Guid.NewGuid();
        var nomeBanco = Guid.NewGuid().ToString();
        Guid orcamentoEstrangeiroId;

        await using (var contextoEstrangeiro = CriarContexto(tenantEstrangeiroId, nomeBanco))
        {
            var prospecto = Prospecto.Create(tenantEstrangeiroId, "Empresa Estrangeira", "55666777000188", null);
            var orcamento = Orcamento.Criar(tenantEstrangeiroId, prospecto.Id, "Proposta estrangeira", 15, null);
            contextoEstrangeiro.Prospects.Add(prospecto);
            contextoEstrangeiro.Budgets.Add(orcamento);
            await contextoEstrangeiro.SaveChangesAsync();
            orcamentoEstrangeiroId = orcamento.Id;
        }

        await using var contexto = CriarContexto(tenantId, nomeBanco);
        var handler = new ObterOrcamentoPorIdHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(new ObterOrcamentoPorIdConsulta(orcamentoEstrangeiroId), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Orcamento nao encontrado", resultado.Erro);
    }

    [Fact]
    public async Task ObterPorProspecto_QuandoProspectoInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var handler = new ObterOrcamentosPorProspectoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new ObterOrcamentosPorProspectoConsulta(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Prospecto nao encontrado", resultado.Erro);
    }

    [Fact]
    public async Task ObterPorProspecto_QuandoProspectoDeOutroTenant_Falha()
    {
        var tenantId = Guid.NewGuid();
        var tenantEstrangeiroId = Guid.NewGuid();
        var nomeBanco = Guid.NewGuid().ToString();
        Guid prospectoEstrangeiroId;

        await using (var contextoEstrangeiro = CriarContexto(tenantEstrangeiroId, nomeBanco))
        {
            var prospectoEstrangeiro = Prospecto.Create(
                tenantEstrangeiroId,
                "Empresa Estrangeira",
                "55666777000188",
                null);
            contextoEstrangeiro.Prospects.Add(prospectoEstrangeiro);
            await contextoEstrangeiro.SaveChangesAsync();
            prospectoEstrangeiroId = prospectoEstrangeiro.Id;
        }

        await using var contexto = CriarContexto(tenantId, nomeBanco);
        var handler = new ObterOrcamentosPorProspectoHandler(CriarServico(contexto, tenantId));

        var resultado = await handler.Handle(
            new ObterOrcamentosPorProspectoConsulta(prospectoEstrangeiroId),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Prospecto nao encontrado", resultado.Erro);
    }

    private static CriarOrcamentoHandler CriarHandler(ContextoAplicacao contexto, Guid tenantId)
        => new(CriarServico(contexto, tenantId));

    private static IOrcamentoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
        => new OrcamentoServico(
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId, string? nomeBanco = null)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(nomeBanco ?? Guid.NewGuid().ToString())
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
