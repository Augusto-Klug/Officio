namespace Officio.Dominio.Interfaces;

public interface IUsuarioAtual
{
    Guid UserId { get; }
    string? Email { get; }
}





