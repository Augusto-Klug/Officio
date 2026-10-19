using Officio.Dominio.Interfaces;
using Officio.Dominio.Eventos;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class PublicadorAtualizacaoTempoRealNulo : IPublicadorAtualizacaoTempoReal
{
    public Task PublicarAsync(
        EventoAtualizacaoTempoReal evento,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
