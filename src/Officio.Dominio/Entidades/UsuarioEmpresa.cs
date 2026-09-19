using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class UsuarioEmpresa : EntidadeBase
{
    public Guid UsuarioId { get; private set; }
    public Guid EmpresaId { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public Usuario Usuario { get; private set; } = default!;
    public Empresa Empresa { get; private set; } = default!;

    private UsuarioEmpresa() { }

    public static UsuarioEmpresa Criar(Guid usuarioId, Guid empresaId)
        => new() { UsuarioId = usuarioId, EmpresaId = empresaId };

    public void AlterarStatus(bool ativo) => Ativo = ativo;
}
