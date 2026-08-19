using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Testes.Unitarios.Dominio;

public class OrcamentoTestes
{
    [Fact]
    public void CalcularTotal_ConsideraSomenteItensMonetarios()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta inicial", 5, null);

        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Escrituracao contabil",
            SecaoItemOrcamento.ServicosInclusos, TipoItemOrcamento.ServicoDescricao, 1, 999m, 1));
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios mensais",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 450m, 2));
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Folha por funcionario",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 3, 50m, 3));

        Assert.Equal(600m, orcamento.ValorTotal);
    }

    [Fact]
    public void Atualizar_QuandoAceito_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Aceitar();

        var erro = Assert.Throws<InvalidOperationException>((Action)(() =>
            orcamento.Atualizar("Nova proposta", 10, "obs")));

        Assert.Contains("nao pode ser editado", erro.Message);
    }

    [Fact]
    public void AdicionarItem_QuandoAceito_LancaErro()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Aceitar();
        var item = ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios mensais",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 450m, 1);

        var erro = Assert.Throws<InvalidOperationException>((Action)(() => orcamento.AdicionarItem(item)));

        Assert.Contains("nao pode ser editado", erro.Message);
    }

    [Fact]
    public void Itens_ExpoeColecaoSomenteLeitura()
    {
        var propriedade = typeof(Orcamento).GetProperty(nameof(Orcamento.Itens));

        Assert.NotNull(propriedade);
        Assert.Equal(typeof(IReadOnlyCollection<ItemOrcamento>), propriedade.PropertyType);
    }

    [Fact]
    public void Itens_Expostos_NaoPodemSerConvertidosParaListaMutavel()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);

        var itensMutaveis = orcamento.Itens as List<ItemOrcamento>;

        Assert.Null(itensMutaveis);
    }

    [Fact]
    public void Recusar_SemMotivo_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);

        var erro = Assert.Throws<ArgumentException>((Action)(() => orcamento.Recusar("")));

        Assert.Contains("motivo", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(StatusOrcamento.Recusado)]
    [InlineData(StatusOrcamento.Expirado)]
    public void Atualizar_QuandoStatusTerminal_LancaErro(StatusOrcamento status)
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        if (status == StatusOrcamento.Recusado)
            orcamento.Recusar("Sem interesse");
        else
            orcamento.AlterarStatus(status);

        var erro = Assert.Throws<InvalidOperationException>(() =>
            orcamento.Atualizar("Nova proposta", 10, "obs"));

        Assert.Contains("nao pode ser editado", erro.Message);
    }

    [Fact]
    public void AlterarStatus_ParaRecusado_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);

        var erro = Assert.Throws<InvalidOperationException>(() =>
            orcamento.AlterarStatus(StatusOrcamento.Recusado));

        Assert.Contains("Aceitar", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AlterarStatus_QuandoAceito_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Aceitar();

        var erro = Assert.Throws<InvalidOperationException>(() =>
            orcamento.AlterarStatus(StatusOrcamento.Rascunho));

        Assert.Contains("terminal", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Recusar_QuandoAceito_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Aceitar();

        var erro = Assert.Throws<InvalidOperationException>(() => orcamento.Recusar("Sem interesse"));

        Assert.Contains("terminal", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Aceitar_QuandoRecusado_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Recusar("Sem interesse");

        var erro = Assert.Throws<InvalidOperationException>(() => orcamento.Aceitar());

        Assert.Contains("terminal", erro.Message, StringComparison.OrdinalIgnoreCase);
    }
}
