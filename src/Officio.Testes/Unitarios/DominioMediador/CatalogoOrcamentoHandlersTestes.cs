using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class CatalogoOrcamentoHandlersTestes
{
    [Fact]
    public async Task CriarItemCatalogo_ComDadosValidos_PersisteItem()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var repositorio = new OrcamentoRepositorio(contexto);
        var empresaAtual = CriarEmpresaAtual(tenantId);
        var handler = new CriarItemCatalogoOrcamentoHandler(repositorio, empresaAtual);
        var comando = new CriarItemCatalogoOrcamentoComando(
            "Escrituracao contabil",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            500m);

        var resultado = await handler.Handle(comando, CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.NotEqual(Guid.Empty, resultado.Valor);
        var item = await contexto.BudgetCatalogItems.SingleAsync();
        Assert.Equal(tenantId, item.TenantId);
        Assert.Equal("Escrituracao contabil", item.Descricao);
    }

    [Fact]
    public async Task ListarCatalogo_RetornaSomenteAtivos()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var repositorio = new OrcamentoRepositorio(contexto);
        var empresaAtual = CriarEmpresaAtual(tenantId);
        var criarHandler = new CriarItemCatalogoOrcamentoHandler(repositorio, empresaAtual);
        var itemAtivo = await criarHandler.Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var itemInativo = await criarHandler.Handle(CriarComando("Folha de pagamento"), CancellationToken.None);
        var alterarHandler = new AlterarAtivoItemCatalogoOrcamentoHandler(repositorio);
        await alterarHandler.Handle(
            new AlterarAtivoItemCatalogoOrcamentoComando(itemInativo.Valor!, false),
            CancellationToken.None);
        var handler = new ListarCatalogoOrcamentoHandler(repositorio);

        var resultado = await handler.Handle(new ListarCatalogoOrcamentoConsulta(), CancellationToken.None);

        var item = Assert.Single(resultado.Valor!);
        Assert.Equal(itemAtivo.Valor, item.Id);
        Assert.True(item.Ativo);
    }

    [Fact]
    public async Task DesativarItemCatalogo_RemoveDaListagemAtiva()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var repositorio = new OrcamentoRepositorio(contexto);
        var empresaAtual = CriarEmpresaAtual(tenantId);
        var criarHandler = new CriarItemCatalogoOrcamentoHandler(repositorio, empresaAtual);
        var itemCriado = await criarHandler.Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var alterarHandler = new AlterarAtivoItemCatalogoOrcamentoHandler(repositorio);

        var resultadoAlteracao = await alterarHandler.Handle(
            new AlterarAtivoItemCatalogoOrcamentoComando(itemCriado.Valor!, false),
            CancellationToken.None);
        var resultadoListagem = await new ListarCatalogoOrcamentoHandler(repositorio)
            .Handle(new ListarCatalogoOrcamentoConsulta(), CancellationToken.None);

        Assert.True(resultadoAlteracao.Sucesso);
        Assert.Empty(resultadoListagem.Valor!);
    }

    [Fact]
    public async Task AtualizarItemCatalogo_ComDadosValidos_PersisteAlteracoes()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var repositorio = new OrcamentoRepositorio(contexto);
        var item = await new CriarItemCatalogoOrcamentoHandler(repositorio, CriarEmpresaAtual(tenantId))
            .Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var handler = new AtualizarItemCatalogoOrcamentoHandler(repositorio);

        var resultado = await handler.Handle(new AtualizarItemCatalogoOrcamentoComando(
            item.Valor!,
            "Folha de pagamento",
            SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.RegraComercial,
            950m), CancellationToken.None);

        Assert.True(resultado.Sucesso);
        var itemPersistido = await contexto.BudgetCatalogItems.SingleAsync();
        Assert.Equal("Folha de pagamento", itemPersistido.Descricao);
        Assert.Equal(SecaoItemOrcamento.ServicosSeparados, itemPersistido.Secao);
        Assert.Equal(TipoItemOrcamento.RegraComercial, itemPersistido.Tipo);
        Assert.Equal(950m, itemPersistido.ValorReferencia);
    }

    [Fact]
    public async Task AtualizarItemCatalogo_QuandoItemInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var handler = new AtualizarItemCatalogoOrcamentoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(new AtualizarItemCatalogoOrcamentoComando(
            Guid.NewGuid(),
            "Folha de pagamento",
            SecaoItemOrcamento.ServicosSeparados,
            TipoItemOrcamento.RegraComercial,
            950m), CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    private static CriarItemCatalogoOrcamentoComando CriarComando(string descricao)
        => new(
            descricao,
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            500m);

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
