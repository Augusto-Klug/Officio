namespace Officio.Dominio.Servicos.Interfaces;

public interface IUsuarioAtual
{
    Guid UserId { get; }
    string? Email { get; }
}





