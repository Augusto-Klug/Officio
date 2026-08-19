using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class OrcamentoStatusHandlersTestes
{
    [Fact]
    public async Task AceitarOrcamento_QuandoRascunho_MarcaComoAceito()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new AceitarOrcamentoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(orcamento.Id),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusOrcamento.Aceito, (await contexto.Budgets.SingleAsync()).Status);
    }

    [Fact]
    public async Task AceitarOrcamento_QuandoJaAceito_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        orcamento.Aceitar();
        await contexto.SaveChangesAsync();
        var handler = new AceitarOrcamentoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(
            new AceitarOrcamentoComando(orcamento.Id),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("aceito", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecusarOrcamento_SemMotivo_Falha()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new RecusarOrcamentoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(
            new RecusarOrcamentoComando(orcamento.Id, ""),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.Contains("motivo", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RecusarOrcamento_ComMotivo_PersisteEExibeMotivo()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new RecusarOrcamentoHandler(new OrcamentoRepositorio(contexto));

        var resultado = await handler.Handle(
            new RecusarOrcamentoComando(orcamento.Id, "Preco acima do esperado"),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        contexto.ChangeTracker.Clear();

        var orcamentoPersistido = await contexto.Budgets.SingleAsync();
        var consultaHandler = new ObterOrcamentosPorProspectoHandler(new OrcamentoRepositorio(contexto));
        var consulta = await consultaHandler.Handle(
            new ObterOrcamentosPorProspectoConsulta(orcamento.ProspectId),
            CancellationToken.None);

        Assert.Equal(StatusOrcamento.Recusado, orcamentoPersistido.Status);
        Assert.Equal("Preco acima do esperado", orcamentoPersistido.MotivoRecusa);
        Assert.Equal("Preco acima do esperado", Assert.Single(consulta.Valor!).MotivoRecusa);
    }

    [Fact]
    public async Task AlterarStatus_ParaEnviado_MarcaProspectoComoPropostaEnviada()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var orcamento = await CriarOrcamentoAsync(contexto, tenantId);
        var handler = new AlterarStatusOrcamentoHandler(
            new OrcamentoRepositorio(contexto),
            new ProspectoRepositorio(contexto));

        var resultado = await handler.Handle(
            new AlterarStatusOrcamentoComando(orcamento.Id, StatusOrcamento.Enviado),
            CancellationToken.None);

        Assert.True(resultado.Sucesso);
        Assert.Equal(StatusProspecto.PropostaEnviada, (await contexto.Prospects.SingleAsync()).Status);
    }

    private static async Task<Orcamento> CriarOrcamentoAsync(ContextoAplicacao contexto, Guid tenantId)
    {
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        contexto.Prospects.Add(prospecto);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();
        return orcamento;
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
