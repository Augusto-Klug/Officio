using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ContratoRepositorioTestes
{
    [Fact]
    public async Task ObterPorOrcamentoAsync_RetornaContratoDoOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (prospecto, orcamento, contrato) = CriarContratoAceito(tenantId);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        var repositorio = new ContratoRepositorio(contexto);

        var resultado = await repositorio.ObterPorOrcamentoAsync(orcamento.Id);

        Assert.NotNull(resultado);
        Assert.Equal(contrato.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterDetalheAsync_CarregaProspectoEOrcamento()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var (prospecto, orcamento, contrato) = CriarContratoAceito(tenantId);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new ContratoRepositorio(contexto);

        var resultado = await repositorio.ObterDetalheAsync(contrato.Id);

        Assert.NotNull(resultado);
        Assert.Equal(prospecto.RazaoSocial, resultado.Prospecto.RazaoSocial);
        Assert.Equal(orcamento.Id, resultado.Orcamento!.Id);
    }

    [Fact]
    public async Task ListarAsync_FiltraEmpresaECarregaOpcoesComPdfAssinadoEmLote()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var empresaA = Empresa.CriarConstituida(tenantId, "11222333000144", "Empresa A");
        var empresaB = Empresa.CriarConstituida(tenantId, "99888777000166", "Empresa B");
        var conjuntoA = CriarContratoAceito(tenantId);
        var conjuntoB = CriarContratoAceito(tenantId);
        conjuntoA.Prospecto.VincularEmpresa(empresaA.Id);
        conjuntoA.Contrato.VincularEmpresa(empresaA.Id);
        conjuntoA.Contrato.MarkAsSigned(new DateTime(2026, 9, 2));
        conjuntoB.Prospecto.VincularEmpresa(empresaB.Id);
        conjuntoB.Contrato.VincularEmpresa(empresaB.Id);
        contexto.Empresas.AddRange(empresaA, empresaB);
        contexto.Prospects.AddRange(conjuntoA.Prospecto, conjuntoB.Prospecto);
        contexto.Budgets.AddRange(conjuntoA.Orcamento, conjuntoB.Orcamento);
        contexto.Contracts.AddRange(conjuntoA.Contrato, conjuntoB.Contrato);
        contexto.Documents.Add(Documento.CriarContratoAssinado(
            tenantId,
            conjuntoA.Contrato.Id,
            empresaA.Id,
            null,
            conjuntoA.Prospecto.Id,
            "contrato-assinado.pdf",
            "contratos/assinado.pdf",
            123));
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var repositorio = new ContratoRepositorio(contexto);

        var contratos = await repositorio.ListarAsync(empresaA.Id, StatusContrato.Ativo);
        var empresas = await repositorio.ListarEmpresasComContratoAsync();
        var idsComPdf = await repositorio.ListarIdsComDocumentoAssinadoAsync(
            [conjuntoA.Contrato.Id, conjuntoB.Contrato.Id]);

        Assert.Equal(conjuntoA.Contrato.Id, Assert.Single(contratos).Id);
        Assert.Equal(["Empresa A", "Empresa B"], empresas.Select(empresa => empresa.RazaoSocial));
        Assert.Equal(conjuntoA.Contrato.Id, Assert.Single(idsComPdf));
    }

    private static (Prospecto Prospecto, Orcamento Orcamento, Contrato Contrato) CriarContratoAceito(Guid tenantId)
    {
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios",
            SecaoItemOrcamento.Honorarios, TipoItemOrcamento.ItemMonetario, 1, 500m, 1));
        orcamento.Aceitar();
        var contrato = Contrato.CriarDeOrcamento(orcamento, prospecto, new DateTime(2026, 9, 1), null, null);
        return (prospecto, orcamento, contrato);
    }

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
