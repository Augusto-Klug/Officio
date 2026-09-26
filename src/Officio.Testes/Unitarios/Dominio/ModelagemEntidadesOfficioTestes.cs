using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;

namespace Officio.Testes.Unitarios.Dominio;

public class ModelagemEntidadesOfficioTestes
{
    [Fact]
    public void Prospecto_NovoModelo_NaoExigeDuplicarPessoaEnderecoOuEmpresa()
    {
        var prospecto = Prospecto.Create(Guid.NewGuid(), "Empresa em abertura", null, RegimeTributario.SimplesNacional);

        Assert.Equal("NOVA_EMPRESA", prospecto.TipoProspect);
        Assert.Null(prospecto.EmpresaId);
        Assert.Empty(prospecto.Pessoas);
        Assert.Null(prospecto.FormularioAbertura);
    }

    [Fact]
    public void ProspectoPessoa_CriaVinculoSemCopiarDadosPessoais()
    {
        var prospectId = Guid.NewGuid();
        var pessoaId = Guid.NewGuid();
        var tipoRelacaoId = Guid.NewGuid();

        var vinculo = ProspectoPessoa.Criar(prospectId, pessoaId, tipoRelacaoId, principal: true, "Contato principal");

        Assert.Equal(prospectId, vinculo.ProspectId);
        Assert.Equal(pessoaId, vinculo.PessoaId);
        Assert.Equal(tipoRelacaoId, vinculo.TipoRelacaoId);
        Assert.True(vinculo.Principal);
    }

    [Fact]
    public void EmpresaConstituida_GuardaCnpjRazaoSocialEQuantidadesOperacionais()
    {
        var parceiroId = Guid.NewGuid();
        var empresa = Empresa.CriarConstituida(parceiroId, "11222333000144", "Officio Cliente Ltda", "Officio", 3, 2);

        Assert.Equal("11222333000144", empresa.Cnpj);
        Assert.Equal("Officio Cliente Ltda", empresa.RazaoSocial);
        Assert.Equal(3, empresa.QuantidadeFuncionariosAtual);
        Assert.Equal(2, empresa.QuantidadeProLaboresAtual);
        Assert.Equal(parceiroId, empresa.TenantId);
    }

    [Fact]
    public void Parceiro_ExigeCnpjComQuatorzeDigitosENormalizaPontuacao()
    {
        var parceiro = Parceiro.Criar("02.757.241/0001-05", "Parisotto Contabilidade Ltda");

        Assert.Equal("02757241000105", parceiro.Cnpj);
        Assert.Throws<ArgumentException>(() => Parceiro.Criar("0275724100010", "Parceiro invalido"));
    }

    [Fact]
    public void ContratoCriadoDeOrcamento_MantemOrigemNoOrcamentoAceito()
    {
        var tenantId = Guid.NewGuid();
        var prospecto = Prospecto.Create(tenantId, "Cliente Ltda", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorario mensal",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        orcamento.Aceitar();

        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, DateTime.UtcNow.Date, null, true, null);

        Assert.Equal(orcamento.Id, contrato.BudgetId);
        Assert.Equal(orcamento.ValorTotal, contrato.ValorTotal);
        Assert.Equal(StatusContrato.Rascunho, contrato.Status);
    }

    [Fact]
    public void FaturamentoMensal_CalculaValorTotalComSnapshotDasQuantidades()
    {
        var faturamento = FaturamentoMensal.Criar(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 1),
            quantidadeFuncionarios: 4,
            quantidadeProLabores: 2,
            valorHonorarioBase: 700m,
            valorPorFolha: 45m);

        Assert.Equal(6, faturamento.QuantidadeFolhas);
        Assert.Equal(970m, faturamento.ValorTotal);
    }
}
