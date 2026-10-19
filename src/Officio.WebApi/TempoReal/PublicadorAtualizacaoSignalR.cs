using Microsoft.AspNetCore.SignalR;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.WebApi.TempoReal;

public sealed class PublicadorAtualizacaoSignalR(
    IHubContext<AtualizacoesHub> hubContext,
    ILogger<PublicadorAtualizacaoSignalR> logger) : IPublicadorAtualizacaoTempoReal
{
    public async Task PublicarAsync(
        EventoAtualizacaoTempoReal evento,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients
                .Group(AtualizacoesHub.GrupoParceiro(evento.ParceiroId))
                .SendAsync(
                    "Atualizacao",
                    new AtualizacaoTempoRealMensagem(evento.Entidade, evento.Id),
                    cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Nao foi possivel publicar atualizacao em tempo real. ParceiroId={ParceiroId} Entidade={Entidade} RegistroId={RegistroId}",
                evento.ParceiroId,
                evento.Entidade,
                evento.Id);
        }
    }
}
