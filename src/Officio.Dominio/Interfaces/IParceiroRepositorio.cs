using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IParceiroRepositorio
{
    Task<Parceiro?> ObterPorIdAsync(Guid parceiroId, CancellationToken cancellationToken = default);
}
