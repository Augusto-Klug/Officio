using MediatR;
using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Comum;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.InfraBancoDeDados;

public class ContextoAplicacaoEventosTestes
{
    [Fact]
    public async Task SaveChangesAsync_QuandoComandoEstaEmExecucao_RegistraAuditoriaNaMesmaPersistencia()
    {
        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        var contextoAuditoria = new ContextoAuditoriaFake("CriarProspectoComando", usuarioId);
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var contexto = new ContextoAplicacao(
            options,
            empresaAtual,
            new PublicadorEventosFake(),
            contextoAuditoria);
        var prospecto = Prospecto.Create(tenantId, "Empresa Auditada", "11222333000144", null);
        contexto.Prospects.Add(prospecto);

        await contexto.SaveChangesAsync();

        var registro = await contexto.AuditLogs.SingleAsync();
        Assert.Equal(tenantId, registro.TenantId);
        Assert.Equal(usuarioId, registro.UserId);
        Assert.Equal(nameof(Prospecto), registro.Entidade);
        Assert.Equal(prospecto.Id, registro.EntidadeId);
        Assert.Equal("CriarProspectoComando:CRIADO", registro.Acao);
    }

    [Fact]
    public async Task SaveChangesAsync_QuandoEntidadePossuiEvento_PublicaEventoELimpaFila()
    {
        var tenantId = Guid.NewGuid();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        var publicador = new PublicadorEventosFake();
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var contexto = new ContextoAplicacao(options, empresaAtual, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste Ltda", "11222333000144", null);
        prospecto.MarkAsConverted(Guid.NewGuid());

        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();

        Assert.Single(publicador.Notificacoes);
        Assert.Empty(prospecto.EventosDominio);
    }

    [Fact]
    public async Task SaveChangesAsync_PublicaEventoDeDominioUmaVez()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();

        prospecto.MarkAsConverted(Guid.NewGuid());

        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoProspectoConvertido>());
        Assert.Equal(prospecto.Id, evento.ProspectId);
    }

    [Fact]
    public async Task SaveChangesAsync_LimpaEventosAposPublicacao()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        contexto.Prospects.Add(prospecto);
        await contexto.SaveChangesAsync();

        prospecto.MarkAsConverted(Guid.NewGuid());
        await contexto.SaveChangesAsync();

        Assert.Empty(prospecto.EventosDominio);
    }

    [Fact]
    public async Task SaveChangesAsync_QuandoContratoAssinado_PublicaEventoContratoAssinado()
    {
        var tenantId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var prospecto = Prospecto.Create(tenantId, "Empresa Teste", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta mensal", 10, null);
        var contrato = Contrato.CreateFromBudget(orcamento, clientId, DateTime.UtcNow, null, null);
        contexto.Prospects.Add(prospecto);
        contexto.Clients.Add(cliente);
        contexto.Budgets.Add(orcamento);
        contexto.Contracts.Add(contrato);
        await contexto.SaveChangesAsync();

        contrato.MarkAsSigned();
        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoContratoAssinado>());
        Assert.Equal(contrato.Id, evento.ContractId);
        Assert.Equal(clientId, evento.ClientId);
        Assert.Equal(tenantId, evento.TenantId);
        Assert.Empty(contrato.EventosDominio);
    }

    [Fact]
    public async Task SaveChangesAsync_QuandoDocumentoValidado_PublicaEventoDocumentoValidado()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var documento = Documento.Create(
            tenantId,
            "cartao-cnpj.pdf",
            TipoDocumento.CartaoCnpj,
            "documentos/cartao-cnpj.pdf",
            0.85m,
            clientId: Guid.NewGuid());
        var validacao = ValidacaoDocumento.Create(
            tenantId,
            documento.Id,
            new Dictionary<string, string> { ["cnpj"] = "11222333000144" },
            [],
            valido: true,
            confiancaOcr: 0.95m);

        contexto.Documents.Add(documento);
        contexto.DocumentValidations.Add(validacao);
        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoDocumentoValidado>());
        Assert.Equal(documento.Id, evento.DocumentId);
        Assert.True(evento.Valido);
        Assert.Equal(tenantId, evento.TenantId);
        Assert.Empty(validacao.EventosDominio);
    }

    [Fact]
    public async Task SaveChangesAsync_QuandoOrcamentoAceito_PublicaEventoOrcamentoAceito()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var orcamento = Orcamento.Criar(tenantId, Guid.NewGuid(), "Proposta", 10, null);
        contexto.Budgets.Add(orcamento);
        await contexto.SaveChangesAsync();

        orcamento.Aceitar();
        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoOrcamentoAceito>());
        Assert.Equal(orcamento.Id, evento.OrcamentoId);
        Assert.Equal(tenantId, evento.TenantId);
    }

    [Fact]
    public async Task SaveChangesAsync_QuandoDocumentoExigeRevisao_PublicaEventoRevisaoManual()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorEventosFake();
        await using var contexto = CriarContexto(tenantId, publicador);
        var documento = Documento.Create(
            tenantId,
            "documento.pdf",
            TipoDocumento.CartaoCnpj,
            "documentos/documento.pdf",
            0.4m,
            clientId: Guid.NewGuid());
        var validacao = ValidacaoDocumento.RequireManualReview(tenantId, documento.Id);
        contexto.Documents.Add(documento);
        contexto.DocumentValidations.Add(validacao);

        await contexto.SaveChangesAsync();

        var evento = Assert.Single(publicador.Notificacoes.OfType<EventoDocumentoRevisaoManual>());
        Assert.Equal(documento.Id, evento.DocumentId);
        Assert.Equal(tenantId, evento.TenantId);
    }

    private static ContextoAplicacao CriarContexto(Guid tenantId, IPublisher publicador)
    {
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ContextoAplicacao(options, empresaAtual, publicador);
    }

    private sealed class ContextoAuditoriaFake(string operacao, Guid usuarioId) : Officio.Dominio.Interfaces.IContextoAuditoria
    {
        public string? OperacaoAtual => operacao;
        public Guid? UsuarioIdAtual => usuarioId;

        public IDisposable Iniciar(string nomeOperacao, Guid? idUsuario) => new EscopoNulo();

        private sealed class EscopoNulo : IDisposable
        {
            public void Dispose() { }
        }
    }
}
