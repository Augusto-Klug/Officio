using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Officio.Infra.BancoDeDados.Contextos;
using Officio.Infra.BancoDeDados.Entidades;

namespace Officio.WebApi.Servicos.Webhooks;

internal sealed class FilaWebhookAutentique(ContextoAplicacao contexto) : IFilaWebhookAutentique
{
    private static readonly TimeSpan DuracaoLease = TimeSpan.FromMinutes(10);
    private const int MaximoTentativas = 8;

    public async Task<ResultadoRecepcaoWebhookAutentique> PersistirAsync(
        string payload,
        ActivityContext contextoTrace,
        CancellationToken cancellationToken)
    {
        var evento = WebhookAutentiqueEvento.Criar(payload, contextoTrace);
        if (await contexto.WebhooksAutentique.AnyAsync(
                existente => existente.HashPayload == evento.HashPayload,
                cancellationToken))
            return ResultadoRecepcaoWebhookAutentique.Duplicado;

        contexto.WebhooksAutentique.Add(evento);
        await contexto.SaveChangesAsync(cancellationToken);
        return ResultadoRecepcaoWebhookAutentique.Persistido;
    }

    public async Task<WebhookAutentiqueEnfileirado?> TentarReservarProximoAsync(
        CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        await contexto.WebhooksAutentique
            .Where(evento => evento.Status == StatusWebhookAutentique.Processando &&
                evento.Tentativas >= MaximoTentativas &&
                (evento.BloqueadoAte == null || evento.BloqueadoAte <= agora))
            .ExecuteUpdateAsync(atualizacoes => atualizacoes
                .SetProperty(evento => evento.Status, StatusWebhookAutentique.Falha)
                .SetProperty(evento => evento.BloqueadoAte, (DateTime?)null)
                .SetProperty(evento => evento.ProximaTentativaEm, (DateTime?)null)
                .SetProperty(evento => evento.UltimoErro,
                    "Limite de tentativas atingido apos expiracao do lease."),
                cancellationToken);

        var candidatos = contexto.WebhooksAutentique
            .Where(evento =>
                (evento.Status == StatusWebhookAutentique.Pendente &&
                 (evento.ProximaTentativaEm == null || evento.ProximaTentativaEm <= agora)) ||
                (evento.Status == StatusWebhookAutentique.Processando &&
                 (evento.BloqueadoAte == null || evento.BloqueadoAte <= agora)))
            .OrderBy(evento => evento.RecebidoEm);

        while (true)
        {
            var id = await candidatos.Select(evento => (Guid?)evento.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (!id.HasValue)
                return null;

            var reservados = await contexto.WebhooksAutentique
                .Where(evento => evento.Id == id.Value &&
                    ((evento.Status == StatusWebhookAutentique.Pendente &&
                      (evento.ProximaTentativaEm == null || evento.ProximaTentativaEm <= agora)) ||
                     (evento.Status == StatusWebhookAutentique.Processando &&
                      (evento.BloqueadoAte == null || evento.BloqueadoAte <= agora))))
                .ExecuteUpdateAsync(atualizacoes => atualizacoes
                    .SetProperty(evento => evento.Status, StatusWebhookAutentique.Processando)
                    .SetProperty(evento => evento.BloqueadoAte, agora.Add(DuracaoLease))
                    .SetProperty(evento => evento.Tentativas, evento => evento.Tentativas + 1),
                    cancellationToken);

            if (reservados == 0)
                continue;

            var reservado = await contexto.WebhooksAutentique.AsNoTracking()
                .Where(evento => evento.Id == id.Value)
                .Select(evento => new
                {
                    evento.Id,
                    evento.Payload,
                    evento.TraceParent,
                    evento.TraceState,
                    evento.Tentativas
                })
                .SingleAsync(cancellationToken);

            if (reservado.Payload is null)
                return null;

            var contextoTrace = default(ActivityContext);
            if (reservado.TraceParent is not null)
                ActivityContext.TryParse(reservado.TraceParent, reservado.TraceState, true, out contextoTrace);

            return new WebhookAutentiqueEnfileirado(
                reservado.Id,
                reservado.Payload,
                contextoTrace,
                reservado.Tentativas);
        }
    }

    public async Task MarcarProcessadoAsync(Guid id, CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        await contexto.WebhooksAutentique
            .Where(evento => evento.Id == id && evento.Status == StatusWebhookAutentique.Processando)
            .ExecuteUpdateAsync(atualizacoes => atualizacoes
                .SetProperty(evento => evento.Status, StatusWebhookAutentique.Processado)
                .SetProperty(evento => evento.ProcessadoEm, agora)
                .SetProperty(evento => evento.BloqueadoAte, (DateTime?)null)
                .SetProperty(evento => evento.Payload, (string?)null)
                .SetProperty(evento => evento.TraceParent, (string?)null)
                .SetProperty(evento => evento.TraceState, (string?)null)
                .SetProperty(evento => evento.UltimoErro, (string?)null),
                cancellationToken);
    }

    public async Task RegistrarFalhaAsync(Guid id, string erro, CancellationToken cancellationToken)
    {
        var evento = await contexto.WebhooksAutentique.AsNoTracking()
            .Where(item => item.Id == id && item.Status == StatusWebhookAutentique.Processando)
            .Select(item => new { item.Tentativas })
            .SingleOrDefaultAsync(cancellationToken);
        if (evento is null)
            return;

        var falhaFinal = evento.Tentativas >= MaximoTentativas;
        var atrasoSegundos = Math.Min(900, 5 * Math.Pow(2, Math.Min(evento.Tentativas - 1, 8)));
        var erroLimitado = erro.Length <= 2000 ? erro : erro[..2000];
        var agora = DateTime.UtcNow;

        await contexto.WebhooksAutentique
            .Where(item => item.Id == id && item.Status == StatusWebhookAutentique.Processando)
            .ExecuteUpdateAsync(atualizacoes => atualizacoes
                .SetProperty(item => item.Status,
                    falhaFinal ? StatusWebhookAutentique.Falha : StatusWebhookAutentique.Pendente)
                .SetProperty(item => item.ProximaTentativaEm,
                    falhaFinal ? (DateTime?)null : agora.AddSeconds(atrasoSegundos))
                .SetProperty(item => item.BloqueadoAte, (DateTime?)null)
                .SetProperty(item => item.UltimoErro, erroLimitado),
                cancellationToken);
    }
}
