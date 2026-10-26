using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarAtivoItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarItemCatalogoOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ListarCatalogoOrcamento;
using Officio.Dominio.Servicos.Servicos;
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
        var handler = new CriarItemCatalogoOrcamentoHandler(CriarServico(contexto, tenantId));
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
        var servico = CriarServico(contexto, tenantId);
        var criarHandler = new CriarItemCatalogoOrcamentoHandler(servico);
        var itemAtivo = await criarHandler.Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var itemInativo = await criarHandler.Handle(CriarComando("Folha de pagamento"), CancellationToken.None);
        var alterarHandler = new AlterarAtivoItemCatalogoOrcamentoHandler(servico);
        await alterarHandler.Handle(
            new AlterarAtivoItemCatalogoOrcamentoComando(itemInativo.Valor!, false),
            CancellationToken.None);
        var handler = new ListarCatalogoOrcamentoHandler(servico);

        var resultado = await handler.Handle(new ListarCatalogoOrcamentoConsulta(), CancellationToken.None);

        var item = Assert.Single(resultado.Valor!);
        Assert.Equal(itemAtivo.Valor, item.Id);
        Assert.True(item.Ativo);

        var incluindoInativos = await handler.Handle(
            new ListarCatalogoOrcamentoConsulta(IncluirInativos: true),
            CancellationToken.None);
        Assert.Equal(2, incluindoInativos.Valor!.Count);
        Assert.Contains(incluindoInativos.Valor, catalogo => catalogo.Id == itemInativo.Valor && !catalogo.Ativo);
    }

    [Fact]
    public async Task DesativarItemCatalogo_RemoveDaListagemAtiva()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var servico = CriarServico(contexto, tenantId);
        var criarHandler = new CriarItemCatalogoOrcamentoHandler(servico);
        var itemCriado = await criarHandler.Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var alterarHandler = new AlterarAtivoItemCatalogoOrcamentoHandler(servico);

        var resultadoAlteracao = await alterarHandler.Handle(
            new AlterarAtivoItemCatalogoOrcamentoComando(itemCriado.Valor!, false),
            CancellationToken.None);
        var resultadoListagem = await new ListarCatalogoOrcamentoHandler(servico)
            .Handle(new ListarCatalogoOrcamentoConsulta(), CancellationToken.None);

        Assert.True(resultadoAlteracao.Sucesso);
        Assert.Empty(resultadoListagem.Valor!);
    }

    [Fact]
    public async Task AtualizarItemCatalogo_ComDadosValidos_PersisteAlteracoes()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var servico = CriarServico(contexto, tenantId);
        var item = await new CriarItemCatalogoOrcamentoHandler(servico)
            .Handle(CriarComando("Escrituracao contabil"), CancellationToken.None);
        var handler = new AtualizarItemCatalogoOrcamentoHandler(servico);

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
        var handler = new AtualizarItemCatalogoOrcamentoHandler(CriarServico(contexto, tenantId));

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

    private static IOrcamentoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
        => new OrcamentoServico(
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId),
            new PublicadorAtualizacaoTempoRealNulo());

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return empresaAtual;
    }
}
