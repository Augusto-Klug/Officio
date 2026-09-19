namespace Officio.Dominio.Interfaces;

public interface IVerificadorAcesso
{
    Task<bool> UsuarioAtivoAsync(Guid usuarioId, CancellationToken ct = default);
    Task<bool> EmpresaAtivaAsync(Guid empresaId, CancellationToken ct = default);
}
