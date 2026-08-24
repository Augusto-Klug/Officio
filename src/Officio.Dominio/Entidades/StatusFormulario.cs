namespace Officio.Dominio.Entidades;

public class StatusFormulario : EntidadeDominio
{
    private StatusFormulario() { }

    public StatusFormulario(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
