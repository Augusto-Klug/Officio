using Officio.Dominio.Eventos;

namespace Officio.Dominio.Interfaces;

public interface IPublicadorAtualizacaoTempoReal
{
    Task PublicarAsync(
        EventoAtualizacaoTempoReal evento,
        CancellationToken cancellationToken = default);
}
