namespace Officio.Dominio.Contratos;

public record ResultadoAutenticacao(
    string Token,
    DateTime ExpiraEm,
    UsuarioAutenticado Usuario);

public record UsuarioAutenticado(
    Guid Id,
    string Nome,
    string Email,
    string Perfil);
