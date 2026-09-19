using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class NotificacaoRepositorio(ContextoAplicacao contexto)
    : RepositorioBase<ContextoAplicacao, Guid>(contexto), INotificacaoRepositorio
{
    public Task<List<Notificacao>> ListarAsync(
        Guid usuarioId,
        bool? lida = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = Contexto.Notifications
            .AsNoTracking()
            .Where(notificacao => notificacao.UserId == usuarioId);

        if (lida.HasValue)
            consulta = consulta.Where(notificacao => notificacao.Lida == lida.Value);

        return consulta
            .OrderByDescending(notificacao => notificacao.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public Task<Notificacao?> ObterAsync(
        Guid notificacaoId,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
        => Contexto.Notifications.FirstOrDefaultAsync(
            notificacao => notificacao.Id == notificacaoId && notificacao.UserId == usuarioId,
            cancellationToken);

    public Task<List<Notificacao>> ListarNaoLidasAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
        => Contexto.Notifications
            .Where(notificacao => notificacao.UserId == usuarioId && !notificacao.Lida)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> ListarUsuariosAtivosDoTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
        => Contexto.Users
            .IgnoreQueryFilters()
            .Where(usuario => usuario.TenantId == tenantId && usuario.Ativo)
            .Select(usuario => usuario.Id)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> ListarUsuariosJaNotificadosAsync(
        Guid tenantId,
        string tipo,
        Guid referenciaId,
        CancellationToken cancellationToken = default)
        => Contexto.Notifications
            .IgnoreQueryFilters()
            .Where(notificacao => notificacao.TenantId == tenantId
                                  && notificacao.Tipo == tipo
                                  && notificacao.ReferenciaId == referenciaId)
            .Select(notificacao => notificacao.UserId)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(
        IReadOnlyCollection<Notificacao> notificacoes,
        CancellationToken cancellationToken = default)
        => await Contexto.Notifications.AddRangeAsync(notificacoes, cancellationToken);
}
