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

public class FormularioAberturaServicoTestes
{
    [Fact]
    public async Task AtualizarAsync_CriaRascunhoComAtividadesNormalizadas()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Nova Empresa", null, null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();
        var servico = new FormularioAberturaServico(new FormularioAberturaRepositorio(contexto));

        var resultado = await servico.AtualizarAsync(CriarRequisicao(prospecto.Id));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Equal("RASCUNHO", resultado.Valor!.Status);
        Assert.Equal("PROPRIO", resultado.Valor.SituacaoImovel);
        var atividade = Assert.Single(resultado.Valor.Atividades);
        Assert.Equal("6201501", atividade.Cnae);
        Assert.Equal(35m, atividade.Percentual);

        contexto.ChangeTracker.Clear();
        var formulario = await contexto.FormulariosAbertura
            .Include(item => item.Atividades)
            .SingleAsync();
        Assert.Equal("Empresa Exemplo", formulario.NomeEmpresaPretendido);
        Assert.Single(formulario.Atividades);
    }

    [Fact]
    public async Task AtualizarAsync_SubstituiAtividadesDoMesmoProspecto()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Nova Empresa", null, null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();
        var servico = new FormularioAberturaServico(new FormularioAberturaRepositorio(contexto));

        await servico.AtualizarAsync(CriarRequisicao(prospecto.Id));
        contexto.ChangeTracker.Clear();
        var atualizado = await servico.AtualizarAsync(CriarRequisicao(prospecto.Id) with
        {
            NomeEmpresaPretendido = "Empresa Atualizada",
            Atividades = [new("Consultoria empresarial", "70.20-4-00", 100m, false, 2)]
        });

        Assert.True(atualizado.Sucesso, atualizado.Erro);
        Assert.Equal("Empresa Atualizada", atualizado.Valor!.NomeEmpresaPretendido);
        Assert.Equal("7020400", Assert.Single(atualizado.Valor.Atividades).Cnae);
        Assert.Equal(1, await contexto.FormulariosAbertura.CountAsync());
        Assert.Equal(1, await contexto.FormularioAberturaAtividades.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_RejeitaProspectoDeEmpresaExistente()
    {
        var tenantId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        var prospecto = Prospecto.Create(tenantId, "Empresa Constituida", "11222333000144", RegimeTributario.SimplesNacional);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();
        var servico = new FormularioAberturaServico(new FormularioAberturaRepositorio(contexto));

        var resultado = await servico.AtualizarAsync(CriarRequisicao(prospecto.Id));

        Assert.False(resultado.Sucesso);
        Assert.Contains("nova empresa", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    private static AtualizarFormularioAberturaRequisicao CriarRequisicao(Guid prospectoId)
        => new(
            prospectoId, null, null, "Empresa Exemplo", "Exemplo", "123", "Edificio Central", "Proprietario",
            "11222333000144", 100m, 50m, 10000m, "contato@exemplo.com", "47999999999", 3, 1, 10,
            "08:00-18:00", 22, "proprio", 2, "Computadores", "ME", "Observacao", "SERVICO",
            "SIMPLES_NACIONAL", [new("Desenvolvimento de sistemas", "62.01-5-01", 35m, true, 1)]);

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
