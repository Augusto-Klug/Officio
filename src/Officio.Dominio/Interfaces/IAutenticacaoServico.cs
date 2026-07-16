using Officio.Dominio.Contratos;

namespace Officio.Dominio.Interfaces;

public interface IAutenticacaoServico
{
    Task<ResultadoAutenticacao?> AutenticarAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default);
}
