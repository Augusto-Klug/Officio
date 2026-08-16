using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IEmpresaRepositorio
{
    Task<Empresa?> ObterPorIdAsync(Guid empresaId, CancellationToken cancellationToken = default);
}
