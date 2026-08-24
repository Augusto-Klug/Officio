namespace Officio.Dominio.Entidades;

public class PerfilUsuario : EntidadeDominio
{
    private PerfilUsuario() { }

    public PerfilUsuario(string codigo, string descricao)
        : base(codigo, descricao) { }
}
