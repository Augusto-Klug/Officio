using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class NotificacaoServico(
    INotificacaoRepositorio notificacaoRepositorio,
    IUsuarioAtual usuarioAtual,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal) : INotificacaoServico
{
    public async Task<Resultado<List<NotificacaoDto>>> ListarAsync(
        bool? lida = null,
        CancellationToken cancellationToken = default)
    {
        if (usuarioAtual.UserId == Guid.Empty)
            return Resultado<List<NotificacaoDto>>.CriarFalha("Usuario autenticado nao identificado.");

        var notificacoes = await notificacaoRepositorio.ListarAsync(
            usuarioAtual.UserId,
            lida,
            cancellationToken);

        return Resultado<List<NotificacaoDto>>.CriarSucesso(notificacoes.Select(Mapear).ToList());
    }

    public async Task<Resultado<bool>> MarcarComoLidaAsync(
        Guid notificacaoId,
        CancellationToken cancellationToken = default)
    {
        if (usuarioAtual.UserId == Guid.Empty)
            return Resultado<bool>.CriarFalha("Usuario autenticado nao identificado.");

        var notificacao = await notificacaoRepositorio.ObterAsync(
            notificacaoId,
            usuarioAtual.UserId,
            cancellationToken);

        if (notificacao is null)
            return Resultado<bool>.CriarFalha("Notificacao nao encontrada.");

        if (notificacao.Lida)
            return Resultado<bool>.CriarSucesso(true);

        notificacao.MarcarComoLida();
        await notificacaoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        await PublicarInvalidacaoAsync(notificacao.TenantId, cancellationToken);

        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<int>> MarcarTodasComoLidasAsync(
        CancellationToken cancellationToken = default)
    {
        if (usuarioAtual.UserId == Guid.Empty)
            return Resultado<int>.CriarFalha("Usuario autenticado nao identificado.");

        var notificacoes = await notificacaoRepositorio.ListarNaoLidasAsync(
            usuarioAtual.UserId,
            cancellationToken);

        foreach (var notificacao in notificacoes)
            notificacao.MarcarComoLida();

        if (notificacoes.Count > 0)
        {
            await notificacaoRepositorio.SalvarAlteracoesAsync(cancellationToken);
            await PublicarInvalidacaoAsync(notificacoes[0].TenantId, cancellationToken);
        }

        return Resultado<int>.CriarSucesso(notificacoes.Count);
    }

    public async Task NotificarTenantAsync(
        Guid tenantId,
        string tipo,
        string titulo,
        string? mensagem,
        Guid referenciaId,
        CancellationToken cancellationToken = default)
    {
        var usuarios = await notificacaoRepositorio.ListarUsuariosAtivosDoTenantAsync(
            tenantId,
            cancellationToken);

        var usuariosNotificados = await notificacaoRepositorio.ListarUsuariosJaNotificadosAsync(
            tenantId,
            tipo,
            referenciaId,
            cancellationToken);

        var jaNotificados = usuariosNotificados.ToHashSet();
        var notificacoes = usuarios
            .Where(usuarioId => !jaNotificados.Contains(usuarioId))
            .Select(usuarioId => Notificacao.Create(
                tenantId,
                usuarioId,
                tipo,
                titulo,
                mensagem,
                referenciaId))
            .ToList();

        if (notificacoes.Count == 0)
            return;

        await notificacaoRepositorio.AdicionarAsync(notificacoes, cancellationToken);
        await notificacaoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        await PublicarInvalidacaoAsync(tenantId, cancellationToken);
    }

    private Task PublicarInvalidacaoAsync(Guid parceiroId, CancellationToken cancellationToken)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(parceiroId, EntidadesAtualizacaoTempoReal.Notificacao),
            cancellationToken);

    private static NotificacaoDto Mapear(Notificacao notificacao)
        => new(
            notificacao.Id,
            notificacao.Tipo,
            notificacao.Titulo,
            notificacao.Mensagem,
            notificacao.ReferenciaId,
            notificacao.Lida,
            notificacao.LidaEm is { } lidaEm ? ComoUtc(lidaEm) : null,
            ComoUtc(notificacao.CriadoEm));

    private static DateTime ComoUtc(DateTime data)
        => data.Kind == DateTimeKind.Utc
            ? data
            : DateTime.SpecifyKind(data, DateTimeKind.Utc);
}
