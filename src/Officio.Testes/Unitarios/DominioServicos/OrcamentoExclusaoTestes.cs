using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Unitarios.DominioMediador;

namespace Officio.Testes.Unitarios.DominioServicos;

public class OrcamentoExclusaoTestes
{
    [Fact]
    public async Task CriarAsync_AposPersistir_PublicaAtualizacaoDoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var catalogoHonorario = ItemCatalogoOrcamento.Criar(
            tenantId, "Honorarios", SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario,
            500m, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        var catalogoFolha = ItemCatalogoOrcamento.Criar(
            tenantId, "Folha", SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario,
            12m, NaturezaCobrancaItemOrcamento.PorFolha);
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.AddRange(catalogoHonorario, catalogoFolha);
        await contexto.SaveChangesAsync();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultado = await servico.CriarAsync(new Officio.Dominio.Contratos.CriarOrcamentoRequisicao(
            prospecto.Id,
            "Proposta",
            10,
            null,
            [
                new Officio.Dominio.Contratos.ItemOrcamentoRequisicao(
                    catalogoHonorario.Id, null, null, null, 1, 500m, 1),
                new Officio.Dominio.Contratos.ItemOrcamentoRequisicao(
                    catalogoFolha.Id, null, null, null, 1, 12m, 2)
            ]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(
            [new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, resultado.Valor)],
            publicador.Eventos);
    }

    [Fact]
    public async Task AtualizarAsync_AposPersistir_PublicaAtualizacaoDoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Original", 10, null);
        var itemHonorario = ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 1, 500m, 1, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        var itemFolha = ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Folha", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 1, 12m, 2, NaturezaCobrancaItemOrcamento.PorFolha);
        orcamento.AdicionarItem(itemHonorario);
        orcamento.AdicionarItem(itemFolha);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultado = await servico.AtualizarAsync(new Officio.Dominio.Contratos.AtualizarOrcamentoRequisicao(
            orcamento.Id,
            "Atualizado",
            15,
            null,
            [
                new Officio.Dominio.Contratos.ItemOrcamentoRequisicao(
                    null, null, null, null, 2, 500m, 1, ItemOrcamentoId: itemHonorario.Id),
                new Officio.Dominio.Contratos.ItemOrcamentoRequisicao(
                    null, null, null, null, 1, 12m, 2, ItemOrcamentoId: itemFolha.Id)
            ]));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(
            [new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, orcamento.Id)],
            publicador.Eventos);
    }

    [Theory]
    [InlineData(false, false, "Honorario mensal e preco por folha")]
    [InlineData(false, true, "Honorario mensal")]
    [InlineData(true, false, "preco por folha")]
    public async Task CriarAsync_SemNaturezasObrigatorias_NaoPersisteOrcamento(
        bool incluirHonorario,
        bool incluirFolha,
        string textoEsperado)
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var catalogoHonorario = ItemCatalogoOrcamento.Criar(
            tenantId, "Honorarios", SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario,
            500m, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        var catalogoFolha = ItemCatalogoOrcamento.Criar(
            tenantId, "Folha", SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario,
            12m, NaturezaCobrancaItemOrcamento.PorFolha);
        var catalogoNaoRecorrente = ItemCatalogoOrcamento.Criar(
            tenantId, "Servico adicional", SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.ItemMonetario, 100m, NaturezaCobrancaItemOrcamento.NaoRecorrente);
        contexto.Prospects.Add(prospecto);
        contexto.BudgetCatalogItems.AddRange(catalogoHonorario, catalogoFolha, catalogoNaoRecorrente);
        await contexto.SaveChangesAsync();
        var publicador = new PublicadorAtualizacaoFake();
        var itens = new List<Officio.Dominio.Contratos.ItemOrcamentoRequisicao>();
        if (incluirHonorario)
            itens.Add(new(catalogoHonorario.Id, null, null, null, 1, 500m, 1));
        if (incluirFolha)
            itens.Add(new(catalogoFolha.Id, null, null, null, 1, 12m, itens.Count + 1));
        if (itens.Count == 0)
            itens.Add(new(catalogoNaoRecorrente.Id, null, null, null, 1, 100m, 1));

        var resultado = await CriarServico(contexto, tenantId, publicador).CriarAsync(
            new Officio.Dominio.Contratos.CriarOrcamentoRequisicao(
                prospecto.Id, "Proposta", 10, null, itens));

        Assert.False(resultado.Sucesso);
        Assert.Contains(textoEsperado, resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await contexto.Budgets.ToListAsync());
        Assert.Empty(publicador.Eventos);
    }

    [Fact]
    public async Task AtualizarAsync_AoRemoverItemObrigatorio_NaoAlteraOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Original", 10, null);
        var honorario = ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 1, 500m, 1, NaturezaCobrancaItemOrcamento.HonorarioMensal);
        var folha = ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Folha", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 1, 12m, 2, NaturezaCobrancaItemOrcamento.PorFolha);
        orcamento.SubstituirItens([honorario, folha]);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var publicador = new PublicadorAtualizacaoFake();

        var resultado = await CriarServico(contexto, tenantId, publicador).AtualizarAsync(
            new Officio.Dominio.Contratos.AtualizarOrcamentoRequisicao(
                orcamento.Id,
                "Alterado",
                15,
                null,
                [new(null, null, null, null, 1, 500m, 1, ItemOrcamentoId: honorario.Id)]));

        Assert.False(resultado.Sucesso);
        Assert.Contains("preco por folha", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        var salvo = await contexto.Budgets.Include(item => item.Itens).SingleAsync();
        Assert.Equal("Original", salvo.Titulo);
        Assert.Equal(2, salvo.Itens.Count);
        Assert.Empty(publicador.Eventos);
    }

    [Fact]
    public async Task ExcluirAsync_OrcamentoRascunho_RemoveOrcamentoEItens()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
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
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultado = await servico.ExcluirAsync(orcamento.Id);

        Assert.True(resultado.Sucesso);
        Assert.Empty(await contexto.Budgets.ToListAsync());
        Assert.Empty(await contexto.BudgetItems.ToListAsync());
        Assert.Contains(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, orcamento.Id), publicador.Eventos);
    }

    [Theory]
    [InlineData(StatusOrcamento.EmRevisao)]
    [InlineData(StatusOrcamento.Aceito)]
    [InlineData(StatusOrcamento.Enviado)]
    public async Task ExcluirAsync_OrcamentoForaDeRascunho_FalhaESemRemover(StatusOrcamento status)
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        if (status == StatusOrcamento.Aceito)
            orcamento.Aceitar();
        else
            orcamento.AlterarStatus(status);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultado = await servico.ExcluirAsync(orcamento.Id);

        Assert.False(resultado.Sucesso);
        Assert.Contains("rascunho", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Single(await contexto.Budgets.ToListAsync());
        Assert.Empty(publicador.Eventos);
    }

    [Fact]
    public async Task ExcluirAsync_OrcamentoInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ExcluirAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExcluirAsync_OrcamentoDeOutroTenant_FalhaSemRemover()
    {
        var tenantId = Guid.NewGuid();
        var outroTenantId = Guid.NewGuid();
        var databaseName = Guid.NewGuid().ToString();
        Guid orcamentoId;

        await using (var contextoOutroTenant = CriarContexto(outroTenantId, databaseName))
        {
            var prospecto = Prospecto.Create(outroTenantId, "Empresa Teste", "11222333000144", null);
            var orcamento = Orcamento.Criar(outroTenantId, prospecto.Id, "Proposta", 10, null);
            contextoOutroTenant.Prospects.Add(prospecto);
            contextoOutroTenant.Budgets.Add(orcamento);
            await contextoOutroTenant.SaveChangesAsync();
            orcamentoId = orcamento.Id;
        }

        await using var contextoAtual = CriarContexto(tenantId, databaseName);
        var resultado = await CriarServico(contextoAtual, tenantId).ExcluirAsync(orcamentoId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await contextoAtual.Budgets.ToListAsync());
    }

    [Fact]
    public async Task AlterarStatusAsync_Enviado_PublicaOrcamentoEProspecto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultado = await servico.AlterarStatusAsync(
            new Officio.Dominio.Contratos.AlterarStatusOrcamentoRequisicao(orcamento.Id, StatusOrcamento.Enviado));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, orcamento.Id), publicador.Eventos);
        Assert.Contains(new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Prospecto, prospecto.Id), publicador.Eventos);
    }

    [Fact]
    public async Task AceitarERecusarAsync_PublicamAtualizacaoDoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var aceito = Orcamento.Criar(tenantId, prospecto.Id, "Aceito", 10, null);
        var recusado = Orcamento.Criar(tenantId, prospecto.Id, "Recusado", 10, null);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.AddRange(aceito, recusado);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);

        var resultadoAceite = await servico.AceitarAsync(aceito.Id);
        var resultadoRecusa = await servico.RecusarAsync(new Officio.Dominio.Contratos.RecusarOrcamentoRequisicao(
            recusado.Id, "Condicoes comerciais"));

        Assert.True(resultadoAceite.Sucesso, resultadoAceite.Erro);
        Assert.True(resultadoRecusa.Sucesso, resultadoRecusa.Erro);
        Assert.Equal(
            [
                new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, aceito.Id),
                new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.Orcamento, recusado.Id)
            ],
            publicador.Eventos);
    }

    [Fact]
    public async Task CatalogoOrcamento_MutacoesBemSucedidas_PublicamAtualizacaoDoCatalogo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var publicador = new PublicadorAtualizacaoFake();
        var servico = CriarServico(contexto, tenantId, publicador);
        var criado = await servico.CriarItemCatalogoAsync(new Officio.Dominio.Contratos.CriarItemCatalogoOrcamentoRequisicao(
            "Honorarios", SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario,
            500m, NaturezaCobrancaItemOrcamento.NaoRecorrente));
        Assert.True(criado.Sucesso, criado.Erro);

        var atualizado = await servico.AtualizarItemCatalogoAsync(new Officio.Dominio.Contratos.AtualizarItemCatalogoOrcamentoRequisicao(
            criado.Valor, "Honorarios atualizados", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 600m, NaturezaCobrancaItemOrcamento.NaoRecorrente));
        Assert.True(atualizado.Sucesso, atualizado.Erro);

        var alterado = await servico.AlterarAtivoItemCatalogoAsync(
            new Officio.Dominio.Contratos.AlterarAtivoItemCatalogoOrcamentoRequisicao(criado.Valor, false));

        Assert.True(alterado.Sucesso, alterado.Erro);
        Assert.Equal(
            [
                new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.CatalogoOrcamento, criado.Valor),
                new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.CatalogoOrcamento, criado.Valor),
                new EventoAtualizacaoTempoReal(tenantId, EntidadesAtualizacaoTempoReal.CatalogoOrcamento, criado.Valor)
            ],
            publicador.Eventos);
    }

    private static IOrcamentoServico CriarServico(
        ContextoAplicacao contexto,
        Guid tenantId,
        IPublicadorAtualizacaoTempoReal? publicador = null)
        => new OrcamentoServico(
            new OrcamentoRepositorio(contexto),
            new ContratoRepositorio(contexto),
            new ServicoAssinaturaOrcamentoFake(),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId),
            publicador ?? new PublicadorAtualizacaoFake());

    private static ContextoAplicacao CriarContexto(Guid tenantId, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var atual = new ServicoEmpresaAtual();
        atual.SetTenant(tenantId);
        return atual;
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
}
