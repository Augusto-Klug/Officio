using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioServicos;

public class OrcamentoExclusaoTestes
{
    [Fact]
    public async Task ExcluirAsync_OrcamentoRascunho_RemoveOrcamentoEItens()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        orcamento.AdicionarItem(ItemOrcamento.CriarSnapshot(
            tenantId,
            orcamento.Id,
            null,
            "Honorarios",
            SecaoItemOrcamento.Honorarios,
            TipoItemOrcamento.ItemMonetario,
            1,
            500m,
            1));
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        contexto.ChangeTracker.Clear();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ExcluirAsync(orcamento.Id);

        Assert.True(resultado.Sucesso);
        Assert.Empty(await contexto.Budgets.ToListAsync());
        Assert.Empty(await contexto.BudgetItems.ToListAsync());
    }

    [Theory]
    [InlineData(StatusOrcamento.EmRevisao)]
    [InlineData(StatusOrcamento.Aceito)]
    [InlineData(StatusOrcamento.Enviado)]
    public async Task ExcluirAsync_OrcamentoForaDeRascunho_FalhaESemRemover(StatusOrcamento status)
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        if (status == StatusOrcamento.Aceito)
            orcamento.Aceitar();
        else
            orcamento.AlterarStatus(status);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ExcluirAsync(orcamento.Id);

        Assert.False(resultado.Sucesso);
        Assert.Contains("rascunho", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Single(await contexto.Budgets.ToListAsync());
    }

    [Fact]
    public async Task ExcluirAsync_OrcamentoInexistente_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var servico = CriarServico(contexto, tenantId);

        var resultado = await servico.ExcluirAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExcluirAsync_OrcamentoDeOutroTenant_FalhaSemRemover()
    {
        var tenantId = Guid.NewGuid();
        var outroTenantId = Guid.NewGuid();
        var databaseName = Guid.NewGuid().ToString();
        Guid orcamentoId;

        await using (var contextoOutroTenant = CriarContexto(outroTenantId, databaseName))
        {
            var prospecto = Prospecto.Create(outroTenantId, "Empresa Teste", "11222333000144", null);
            var orcamento = Orcamento.Criar(outroTenantId, prospecto.Id, "Proposta", 10, null);
            contextoOutroTenant.Prospects.Add(prospecto);
            contextoOutroTenant.Budgets.Add(orcamento);
            await contextoOutroTenant.SaveChangesAsync();
            orcamentoId = orcamento.Id;
        }

        await using var contextoAtual = CriarContexto(tenantId, databaseName);
        var resultado = await CriarServico(contextoAtual, tenantId).ExcluirAsync(orcamentoId);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao encontrado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await contextoAtual.Budgets.ToListAsync());
    }

    private static IOrcamentoServico CriarServico(ContextoAplicacao contexto, Guid tenantId)
        => new OrcamentoServico(
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto),
            CriarEmpresaAtual(tenantId));

    private static ContextoAplicacao CriarContexto(Guid tenantId, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
        return new ContextoAplicacao(options, CriarEmpresaAtual(tenantId), new PublicadorEventosFake());
    }

    private static ServicoEmpresaAtual CriarEmpresaAtual(Guid tenantId)
    {
        var atual = new ServicoEmpresaAtual();
        atual.SetTenant(tenantId);
        return atual;
    }
}
