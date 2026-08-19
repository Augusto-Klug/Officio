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
    public void Recusar_SemMotivo_LancaErro()
    {
        var orcamento = Orcamento.Criar(Guid.NewGuid(), Guid.NewGuid(), "Proposta", 5, null);

        var erro = Assert.Throws<ArgumentException>((Action)(() => orcamento.Recusar("")));

        Assert.Contains("motivo", erro.Message, StringComparison.OrdinalIgnoreCase);
    }
}
