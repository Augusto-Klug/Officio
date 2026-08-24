using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class PerfilPermissao : EntidadeBase
{
    public Guid PerfilUsuarioId { get; private set; }
    public Guid PermissaoId { get; private set; }
    public PerfilUsuario PerfilUsuario { get; private set; } = default!;
    public Permissao Permissao { get; private set; } = default!;

    private PerfilPermissao() { }

    public static PerfilPermissao Criar(Guid perfilUsuarioId, Guid permissaoId)
        => new() { PerfilUsuarioId = perfilUsuarioId, PermissaoId = permissaoId };
}
