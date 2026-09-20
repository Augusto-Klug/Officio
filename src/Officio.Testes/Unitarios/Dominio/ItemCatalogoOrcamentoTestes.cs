using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Testes.Unitarios.Dominio;

public class ItemCatalogoOrcamentoTestes
{
    [Fact]
    public void Criar_ComDadosValidos_CriaAtivo()
    {
        var tenantId = Guid.NewGuid();

        var item = ItemCatalogoOrcamento.Criar(
            tenantId,
            "Escrituracao contabil mensal",
            SecaoItemOrcamento.ServicosInclusos,
            TipoItemOrcamento.ServicoDescricao,
            null);

        Assert.Equal(tenantId, item.TenantId);
        Assert.Equal("Escrituracao contabil mensal", item.Descricao);
        Assert.Equal(SecaoItemOrcamento.ServicosInclusos, item.Secao);
        Assert.Equal(TipoItemOrcamento.ServicoDescricao, item.Tipo);
        Assert.True(item.Ativo);
    }

    [Fact]
    public void Desativar_QuandoAtivo_MarcaComoInativo()
    {
        var item = ItemCatalogoOrcamento.Criar(
            Guid.NewGuid(),
            "Honorarios mensais",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            450m);

        item.Desativar();

        Assert.False(item.Ativo);
    }

    [Fact]
    public void Criar_ItemNaoMonetarioRecorrente_RejeitaClassificacao()
    {
        var excecao = Assert.Throws<ArgumentException>(() => ItemCatalogoOrcamento.Criar(
            Guid.NewGuid(),
            "Obrigacao acessoria",
            SecaoItemOrcamento.ServicosInclusos,
            TipoItemOrcamento.ServicoDescricao,
            null,
            NaturezaCobrancaItemOrcamento.HonorarioMensal));

        Assert.Contains("monetarios", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }
}
