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
    public void AdicionarItem_PermiteUmHonorarioMensalEMantemValorUnitario()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);
        var mensal = CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.HonorarioMensal, 1, 800m, 1);

        orcamento.AdicionarItem(mensal);

        Assert.Equal(800m, Assert.Single(orcamento.Itens).ValorUnitario);
        Assert.Equal(800m, orcamento.ValorTotal);
    }

    [Fact]
    public void AdicionarItem_BloqueiaSegundoHonorarioMensal()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);
        orcamento.AdicionarItem(CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.HonorarioMensal, 1, 800m, 1));

        var erro = Assert.Throws<InvalidOperationException>(() =>
            orcamento.AdicionarItem(CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.HonorarioMensal, 1, 900m, 2)));

        Assert.Contains("Honorario mensal", erro.Message);
        Assert.Single(orcamento.Itens);
    }

    [Fact]
    public void AdicionarItem_PermiteUmPorFolhaEPreservaPrecoUnitario()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);
        var porFolha = CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.PorFolha, 25, 3.5m, 1);

        orcamento.AdicionarItem(porFolha);

        Assert.Equal(3.5m, porFolha.ValorUnitario);
        Assert.Equal(87.5m, porFolha.ValorTotal);
    }

    [Fact]
    public void AdicionarItem_BloqueiaSegundoPorFolha()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);
        orcamento.AdicionarItem(CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.PorFolha, 25, 3.5m, 1));

        var erro = Assert.Throws<InvalidOperationException>(() =>
            orcamento.AdicionarItem(CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.PorFolha, 30, 4m, 2)));

        Assert.Contains("Por folha", erro.Message);
        Assert.Single(orcamento.Itens);
    }

    [Fact]
    public void SubstituirItens_PermiteVariosItensNaoRecorrentes()
    {
        var tenantId = Guid.NewGuid();
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);

        orcamento.SubstituirItens([
            CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.NaoRecorrente, 1, 150m, 1),
            CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.NaoRecorrente, 2, 75m, 2),
            CriarItem(tenantId, orcamento.Id, NaturezaCobrancaItemOrcamento.NaoRecorrente, 1, 40m, 3)
        ]);

        Assert.Equal(3, orcamento.Itens.Count);
        Assert.Equal(340m, orcamento.ValorTotal);
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
    public void Substituir_QuandoAceito_TornaStatusTerminal()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 10, null);
        orcamento.Aceitar();

        orcamento.Substituir();

        Assert.Equal(StatusOrcamento.Substituido, orcamento.Status);
        Assert.Throws<InvalidOperationException>(() => orcamento.Aceitar());
        Assert.Throws<InvalidOperationException>(() => orcamento.Atualizar("Outra proposta", 10, null));
    }

    [Fact]
    public void Aceitar_QuandoRecusado_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);
        orcamento.Recusar("Sem interesse");

        var erro = Assert.Throws<InvalidOperationException>(() => orcamento.Aceitar());

        Assert.Contains("terminal", erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ItemOrcamento CriarItem(
        Guid tenantId,
        Guid orcamentoId,
        NaturezaCobrancaItemOrcamento natureza,
        int quantidade,
        decimal valorUnitario,
        int ordem)
        => ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamentoId,
            null,
            natureza.ToString(),
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            quantidade,
            valorUnitario,
            ordem,
            natureza);
}
