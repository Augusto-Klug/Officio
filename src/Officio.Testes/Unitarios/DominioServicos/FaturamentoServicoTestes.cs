using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;
using Officio.Testes.Dubles;

namespace Officio.Testes.Unitarios.DominioServicos;

public class FaturamentoServicoTestes
{
    [Fact]
    public async Task GerarCompetenciaAsync_CriaSnapshotEIgnoraContratoJaCalculado()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 800m, 4, 1);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));

        var primeiro = await servico.GerarCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(new DateOnly(2026, 9, 20)));
        var segundo = await servico.GerarCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(new DateOnly(2026, 9, 1)));

        Assert.True(primeiro.Sucesso, primeiro.Erro);
        Assert.Equal(1, primeiro.Valor!.Gerados);
        Assert.Equal(800m, primeiro.Valor.ValorTotal);
        Assert.Equal(0, segundo.Valor!.Gerados);
        Assert.Equal(1, segundo.Valor.Ignorados);

        contexto.ChangeTracker.Clear();
        var faturamento = await contexto.FaturamentosMensais.Include(item => item.StatusFaturamento).SingleAsync();
        Assert.Equal(new DateOnly(2026, 9, 1), faturamento.Competencia);
        Assert.Equal(4, faturamento.QuantidadeFuncionarios);
        Assert.Equal(1, faturamento.QuantidadeProLabores);
        Assert.Equal(5, faturamento.QuantidadeFolhas);
        Assert.Equal("ABERTO", faturamento.StatusFaturamento.Codigo);
    }

    [Fact]
    public async Task FecharAsync_ImpedeCancelamentoPosterior()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 500m, 2, 0);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));
        await servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(new DateOnly(2026, 9, 1)));
        var faturamentoId = await contexto.FaturamentosMensais.Select(item => item.Id).SingleAsync();

        var fechamento = await servico.FecharAsync(faturamentoId);
        var cancelamento = await servico.CancelarAsync(faturamentoId);

        Assert.True(fechamento.Sucesso, fechamento.Erro);
        Assert.False(cancelamento.Sucesso);
        Assert.Contains("fechado", cancelamento.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GerarCompetenciaAsync_UsaSomenteItensRecorrentesECalculaValorPorFolha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 800m, 4, 1, 20m, 300m);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));

        var resultado = await servico.GerarCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(new DateOnly(2026, 9, 1)));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(900m, resultado.Valor!.ValorTotal);
        var faturamento = await contexto.FaturamentosMensais.SingleAsync();
        Assert.Equal(800m, faturamento.ValorHonorarioBase);
        Assert.Equal(20m, faturamento.ValorPorFolha);
        Assert.Equal(900m, faturamento.ValorTotal);
    }

    [Fact]
    public async Task RecalcularCompetenciaAsync_AtualizaSomenteFaturamentoAberto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 800m, 4, 1, 20m);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));
        var competencia = new DateOnly(2026, 9, 1);
        await servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(competencia));
        var empresa = await contexto.Empresas.SingleAsync();
        empresa.AtualizarQuantidadesOperacionais(6, 2);
        await contexto.SaveChangesAsync();

        var resultado = await servico.RecalcularCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(competencia));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(1, resultado.Valor!.Recalculados);
        Assert.Equal(0, resultado.Valor.Ignorados);
        Assert.Equal(960m, resultado.Valor.ValorTotal);
        contexto.ChangeTracker.Clear();
        var faturamento = await contexto.FaturamentosMensais.SingleAsync();
        Assert.Equal(6, faturamento.QuantidadeFuncionarios);
        Assert.Equal(2, faturamento.QuantidadeProLabores);
        Assert.Equal(8, faturamento.QuantidadeFolhas);
        Assert.Equal(960m, faturamento.ValorTotal);
    }

    [Fact]
    public async Task RecalcularCompetenciaAsync_PreservaFaturamentoFechado()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 500m, 2, 0, 10m);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));
        var competencia = new DateOnly(2026, 9, 1);
        await servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(competencia));
        var faturamentoId = await contexto.FaturamentosMensais.Select(item => item.Id).SingleAsync();
        await servico.FecharAsync(faturamentoId);
        var empresa = await contexto.Empresas.SingleAsync();
        empresa.AtualizarQuantidadesOperacionais(20, 0);
        await contexto.SaveChangesAsync();

        var resultado = await servico.RecalcularCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(competencia));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal(0, resultado.Valor!.Recalculados);
        Assert.Equal(1, resultado.Valor.Ignorados);
        contexto.ChangeTracker.Clear();
        var faturamento = await contexto.FaturamentosMensais.SingleAsync();
        Assert.Equal(2, faturamento.QuantidadeFolhas);
        Assert.Equal(520m, faturamento.ValorTotal);
    }

    [Fact]
    public async Task ObterResumoAsync_SeparaStatusEExcluiCanceladosDoTotalAtivo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 500m, 2, 0);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));
        var competencia = new DateOnly(2026, 9, 1);
        await servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(competencia));

        var resumo = await servico.ObterResumoAsync(competencia);

        Assert.True(resumo.Sucesso, resumo.Erro);
        Assert.Equal(1, resumo.Valor!.TotalRegistros);
        Assert.Equal(1, resumo.Valor.Abertos);
        Assert.Equal(0, resumo.Valor.Fechados);
        Assert.Equal(0, resumo.Valor.Cancelados);
        Assert.Equal(2, resumo.Valor.QuantidadeFolhas);
        Assert.Equal(500m, resumo.Valor.ValorAberto);
        Assert.Equal(500m, resumo.Valor.ValorTotalAtivo);
    }

    [Fact]
    public async Task FecharCompetenciaAsync_FechaAbertosEPermaneceIdempotente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        AdicionarContratoAtivo(contexto, tenantId, 700m, 3, 1, 15m);
        await contexto.SaveChangesAsync();
        var servico = new FaturamentoServico(new FaturamentoRepositorio(contexto));
        var competencia = new DateOnly(2026, 9, 1);
        await servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(competencia));

        var primeiro = await servico.FecharCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(competencia));
        contexto.ChangeTracker.Clear();
        var segundo = await servico.FecharCompetenciaAsync(
            new GerarFaturamentoCompetenciaRequisicao(competencia));

        Assert.True(primeiro.Sucesso, primeiro.Erro);
        Assert.Equal(1, primeiro.Valor!.Fechados);
        Assert.Equal(0, primeiro.Valor.Ignorados);
        Assert.Equal(760m, primeiro.Valor.ValorTotal);
        Assert.Equal(0, segundo.Valor!.Fechados);
        Assert.Equal(1, segundo.Valor.Ignorados);
        contexto.ChangeTracker.Clear();
        var faturamento = await contexto.FaturamentosMensais.Include(item => item.StatusFaturamento).SingleAsync();
        Assert.Equal("FECHADO", faturamento.StatusFaturamento.Codigo);
    }

    private static void AdicionarContratoAtivo(
        ContextoAplicacao contexto,
        Guid tenantId,
        decimal honorario,
        int funcionarios,
        int proLabores,
        decimal valorPorFolha = 0m,
        decimal valorNaoRecorrente = 0m)
    {
        var empresa = Empresa.CriarConstituida(
            tenantId, "11222333000144", "Empresa Cliente", quantidadeFuncionariosAtual: funcionarios,
            quantidadeProLaboresAtual: proLabores);
        var prospecto = Prospecto.Create(tenantId, empresa.RazaoSocial, empresa.Cnpj, null);
        prospecto.VincularEmpresa(empresa.Id);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Mensalidade", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId, orcamento.Id, null, "Honorarios", SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario, 1, honorario, 1,
            NaturezaCobrancaItemOrcamento.HonorarioMensal));
        if (valorPorFolha > 0)
        {
            orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
                tenantId, orcamento.Id, null, "Processamento por folha", SecaoItemOrcamento.Honorarios,
                TipoItemOrcamento.ItemMonetario, 1, valorPorFolha, 2,
                NaturezaCobrancaItemOrcamento.PorFolha));
        }
        if (valorNaoRecorrente > 0)
        {
            orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
                tenantId, orcamento.Id, null, "Implantacao", SecaoItemOrcamento.ServicosSeparados,
                TipoItemOrcamento.ItemMonetario, 1, valorNaoRecorrente, 3,
                NaturezaCobrancaItemOrcamento.NaoRecorrente));
        }
        var contrato = Contrato.CriarDeOrcamento(
            orcamento, prospecto, new DateTime(2026, 1, 1), null, true, null);
        contrato.VincularCliente(cliente.Id);
        contrato.MarkAsSigned(new DateTime(2026, 1, 2));

        contexto.Empresas.Add(empresa);
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }
}
