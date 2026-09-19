using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarComoLida;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Comandos.MarcarTodasComoLidas;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Eventos;
using Officio.Dominio.Mediador.Funcionalidades.Notificacoes.Consultas.ListarNotificacoes;
using Officio.Dominio.Servicos.Servicos;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Repositorios;
using Officio.Infraestrutura.Identidade;

namespace Officio.Testes.Unitarios.DominioMediador;

public class NotificacaoHandlersTestes
{
    [Fact]
    public async Task Listar_RetornaSomenteNotificacoesDoUsuarioAtual()
    {
        var tenantId = Guid.NewGuid();
        var usuarioAtualId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();
        await using var contexto = CriarContexto(tenantId);
        contexto.Notifications.AddRange(
            Notificacao.Create(tenantId, usuarioAtualId, "TIPO", "Minha notificacao"),
            Notificacao.Create(tenantId, outroUsuarioId, "TIPO", "Outra notificacao"));
        await contexto.SaveChangesAsync();
        var handler = new ListarNotificacoesHandler(CriarServico(contexto, usuarioAtualId));

        var resultado = await handler.Handle(
            new ListarNotificacoesConsulta(null),
            CancellationToken.None);

        var notificacao = Assert.Single(resultado.Valor!);
        Assert.Equal("Minha notificacao", notificacao.Titulo);
    }

    [Fact]
    public async Task Listar_ComFiltroNaoLida_RetornaSomentePendentes()
    {
        var tenantId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var lida = Notificacao.Create(tenantId, usuarioId, "TIPO_1", "Lida");
        lida.MarcarComoLida();
        var pendente = Notificacao.Create(tenantId, usuarioId, "TIPO_2", "Pendente");
        await using var contexto = CriarContexto(tenantId);
        contexto.Notifications.AddRange(lida, pendente);
        await contexto.SaveChangesAsync();
        var handler = new ListarNotificacoesHandler(CriarServico(contexto, usuarioId));

        var resultado = await handler.Handle(
            new ListarNotificacoesConsulta(false),
            CancellationToken.None);

        var notificacao = Assert.Single(resultado.Valor!);
        Assert.Equal("Pendente", notificacao.Titulo);
        Assert.False(notificacao.Lida);
    }

    [Fact]
    public async Task MarcarComoLida_NaoPermiteAlterarNotificacaoDeOutroUsuario()
    {
        var tenantId = Guid.NewGuid();
        var usuarioAtualId = Guid.NewGuid();
        var notificacao = Notificacao.Create(
            tenantId,
            Guid.NewGuid(),
            "TIPO",
            "Outra notificacao");
        await using var contexto = CriarContexto(tenantId);
        contexto.Notifications.Add(notificacao);
        await contexto.SaveChangesAsync();
        var handler = new MarcarNotificacaoComoLidaHandler(CriarServico(contexto, usuarioAtualId));

        var resultado = await handler.Handle(
            new MarcarNotificacaoComoLidaComando(notificacao.Id),
            CancellationToken.None);

        Assert.False(resultado.Sucesso);
        Assert.False(notificacao.Lida);
    }

    [Fact]
    public async Task MarcarTodasComoLidas_AlteraSomenteNotificacoesDoUsuarioAtual()
    {
        var tenantId = Guid.NewGuid();
        var usuarioAtualId = Guid.NewGuid();
        var minhaPrimeira = Notificacao.Create(tenantId, usuarioAtualId, "TIPO_1", "Primeira");
        var minhaSegunda = Notificacao.Create(tenantId, usuarioAtualId, "TIPO_2", "Segunda");
        var deOutroUsuario = Notificacao.Create(tenantId, Guid.NewGuid(), "TIPO", "Outra");
        await using var contexto = CriarContexto(tenantId);
        contexto.Notifications.AddRange(minhaPrimeira, minhaSegunda, deOutroUsuario);
        await contexto.SaveChangesAsync();
        var handler = new MarcarTodasNotificacoesComoLidasHandler(
            CriarServico(contexto, usuarioAtualId));

        var resultado = await handler.Handle(
            new MarcarTodasNotificacoesComoLidasComando(),
            CancellationToken.None);

        Assert.Equal(2, resultado.Valor);
        Assert.True(minhaPrimeira.Lida);
        Assert.True(minhaSegunda.Lida);
        Assert.False(deOutroUsuario.Lida);
    }

    [Fact]
    public async Task EventoContratoAssinado_NotificaUsuariosAtivosDoTenant()
    {
        var tenantId = Guid.NewGuid();
        var primeiroUsuario = Usuario.Create(tenantId, "Primeiro", "primeiro@officio.test", "hash", "Admin");
        var segundoUsuario = Usuario.Create(tenantId, "Segundo", "segundo@officio.test", "hash", "Admin");
        await using var contexto = CriarContexto(tenantId);
        contexto.Users.AddRange(primeiroUsuario, segundoUsuario);
        await contexto.SaveChangesAsync();
        var handler = new NotificarContratoAssinadoHandler(
            CriarServico(contexto, primeiroUsuario.Id));
        var contratoId = Guid.NewGuid();

        await handler.Handle(
            new EventoContratoAssinado(contratoId, null, tenantId),
            CancellationToken.None);

        var notificacoes = await contexto.Notifications.AsNoTracking().ToListAsync();
        Assert.Equal(2, notificacoes.Count);
        Assert.All(notificacoes, notificacao =>
        {
            Assert.Equal("CONTRATO_ASSINADO", notificacao.Tipo);
            Assert.Equal(contratoId, notificacao.ReferenciaId);
        });
    }

    [Fact]
    public async Task NotificarTenant_NaoIncluiUsuarioDeOutroTenant()
    {
        var tenantId = Guid.NewGuid();
        var outroTenantId = Guid.NewGuid();
        var usuarioTenant = Usuario.Create(tenantId, "Usuario", "usuario@officio.test", "hash", "Admin");
        var usuarioOutroTenant = Usuario.Create(outroTenantId, "Outro", "outro@officio.test", "hash", "Admin");
        await using var contexto = CriarContexto(tenantId);
        contexto.Users.AddRange(usuarioTenant, usuarioOutroTenant);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, usuarioTenant.Id);

        await servico.NotificarTenantAsync(
            tenantId,
            "EVENTO",
            "Titulo",
            null,
            Guid.NewGuid());

        var notificacao = Assert.Single(await contexto.Notifications.AsNoTracking().ToListAsync());
        Assert.Equal(usuarioTenant.Id, notificacao.UserId);
    }

    [Fact]
    public async Task NotificarTenant_QuandoEventoForReprocessado_NaoDuplicaNotificacao()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Usuario", "usuario@officio.test", "hash", "Admin");
        await using var contexto = CriarContexto(tenantId);
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
        var servico = CriarServico(contexto, usuario.Id);
        var referenciaId = Guid.NewGuid();

        await servico.NotificarTenantAsync(
            tenantId,
            "EVENTO",
            "Titulo",
            null,
            referenciaId);
        await servico.NotificarTenantAsync(
            tenantId,
            "EVENTO",
            "Titulo",
            null,
            referenciaId);

        Assert.Single(await contexto.Notifications.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task EventoContratoAssinado_PublicadoPeloContexto_CriaNotificacaoSemRecursao()
    {
        var tenantId = Guid.NewGuid();
        var publicador = new PublicadorDelegado();
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new ContextoAplicacao(options, empresaAtual, publicador);
        var usuario = Usuario.Create(tenantId, "Usuario", "usuario@officio.test", "hash", "Admin");
        var prospecto = Prospecto.Create(tenantId, "Empresa", "11222333000144", null);
        var cliente = Cliente.CreateFromProspect(prospecto);
        var orcamento = Orcamento.Criar(tenantId, prospecto.Id, "Proposta", 10, null);
        var contrato = Contrato.CreateFromBudget(orcamento, cliente.Id, DateTime.UtcNow, null, true, null);
        contexto.AddRange(usuario, prospecto, cliente, orcamento, contrato);
        await contexto.SaveChangesAsync();
        var handler = new NotificarContratoAssinadoHandler(CriarServico(contexto, usuario.Id));
        publicador.PublicarAsync = notification => notification is EventoContratoAssinado evento
            ? handler.Handle(evento, CancellationToken.None)
            : Task.CompletedTask;

        contrato.MarkAsSigned();
        await contexto.SaveChangesAsync();

        var notificacao = Assert.Single(await contexto.Notifications.AsNoTracking().ToListAsync());
        Assert.Equal("CONTRATO_ASSINADO", notificacao.Tipo);
        Assert.Equal(contrato.Id, notificacao.ReferenciaId);
        Assert.Empty(contrato.EventosDominio);
        Assert.Equal(1, publicador.QuantidadePublicacoes);
    }

    [Fact]
    public async Task EventoDocumentoValidado_QuandoValido_NaoCriaNotificacao()
    {
        var tenantId = Guid.NewGuid();
        var usuario = Usuario.Create(tenantId, "Usuario", "usuario@officio.test", "hash", "Admin");
        await using var contexto = CriarContexto(tenantId);
        contexto.Users.Add(usuario);
        await contexto.SaveChangesAsync();
        var handler = new NotificarDocumentoReprovadoHandler(CriarServico(contexto, usuario.Id));

        await handler.Handle(
            new EventoDocumentoValidado(Guid.NewGuid(), true, tenantId),
            CancellationToken.None);

        Assert.Empty(await contexto.Notifications.AsNoTracking().ToListAsync());
    }

    private static NotificacaoServico CriarServico(
        ContextoAplicacao contexto,
        Guid usuarioId)
        => new(new NotificacaoRepositorio(contexto), new UsuarioAtualFake(usuarioId));

    private static ContextoAplicacao CriarContexto(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<ContextoAplicacao>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var empresaAtual = new ServicoEmpresaAtual();
        empresaAtual.SetTenant(tenantId);

        return new ContextoAplicacao(options, empresaAtual, new PublicadorEventosFake());
    }

    private sealed class UsuarioAtualFake(Guid userId) : IUsuarioAtual
    {
        public Guid UserId { get; } = userId;
        public string? Email => "usuario@officio.test";
    }

    private sealed class PublicadorDelegado : MediatR.IPublisher
    {
        public Func<object, Task> PublicarAsync { get; set; } = _ => Task.CompletedTask;
        public int QuantidadePublicacoes { get; private set; }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            QuantidadePublicacoes++;
            return PublicarAsync(notification);
        }

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : MediatR.INotification
            => Publish((object)notification, cancellationToken);
    }
}
