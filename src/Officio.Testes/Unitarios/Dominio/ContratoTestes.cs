using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Testes.Unitarios.Dominio;

public class ContratoTestes
{
    [Fact]
    public void MarkAsSigned_ContratoCanceladoNaoPodeSerReativado()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, DateTime.UtcNow, null, null);
        contrato.MarcarAguardandoAssinatura();
        contrato.Cancelar();

        contrato.MarkAsSigned();

        Assert.Equal(StatusContrato.Cancelado, contrato.Status);
        Assert.Null(contrato.AssinadoEm);
    }

    [Fact]
    public void CriarDeOrcamento_ComOrcamentoAceito_CriaContratoRascunhoVinculadoAoProspecto()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios mensais",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        orcamento.Aceitar();

        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            new DateTime(2026, 9, 1),
            null,
            "Clausula extra");

        Assert.Equal(tenantId, contrato.TenantId);
        Assert.Equal(prospecto.Id, contrato.ProspectId);
        Assert.Null(contrato.ClientId);
        Assert.Equal(orcamento.Id, contrato.BudgetId);
        Assert.Equal(StatusContrato.Rascunho, contrato.Status);
        Assert.Equal(500m, contrato.ValorTotal);
        Assert.Equal(10, contrato.DiaVencimento);
        Assert.Equal("Clausula extra", contrato.ClausulasExtras);
        Assert.StartsWith("CTR-", contrato.Numero);
    }

    [Fact]
    public void CriarDeOrcamento_ComProspectoDiferente_LancaErro()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var outroProspecto = Prospecto.Create(tenantId, "Outra Empresa", "55666777000188", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();

        var erro = Assert.Throws<InvalidOperationException>(() =>
            Contrato.CriarDeOrcamento(orcamento, outroProspecto, DateTime.UtcNow, null, null));

        Assert.Contains("Prospecto", erro.Message);
    }

    [Fact]
    public void VincularCliente_DeveSerIdempotenteParaMesmoClienteERejeitarOutro()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            DateTime.UtcNow,
            null,
            null);
        var clienteId = Guid.NewGuid();

        contrato.VincularCliente(clienteId);
        contrato.VincularCliente(clienteId);

        Assert.Equal(clienteId, contrato.ClientId);
        Assert.Throws<InvalidOperationException>(() => contrato.VincularCliente(Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => contrato.VincularCliente(Guid.Empty));
    }

    [Fact]
    public void MarkAsSigned_ContratoEncerradoNaoDeveSerReativadoPorEventoRepetido()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, DateTime.UtcNow, null, null);
        var primeiraAssinatura = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

        contrato.MarkAsSigned(primeiraAssinatura);
        contrato.Encerrar();
        contrato.MarkAsSigned(primeiraAssinatura.AddDays(1));

        Assert.Equal(StatusContrato.Encerrado, contrato.Status);
        Assert.Equal(primeiraAssinatura, contrato.AssinadoEm);
    }
}
