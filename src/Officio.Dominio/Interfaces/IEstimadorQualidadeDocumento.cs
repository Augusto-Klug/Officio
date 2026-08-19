namespace Officio.Dominio.Interfaces;

public interface IEstimadorQualidadeDocumento
{
    Task<decimal> EstimarAsync(Stream arquivo, CancellationToken cancellationToken = default);
}
