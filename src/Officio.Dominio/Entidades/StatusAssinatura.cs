namespace Officio.Dominio.Entidades;

public class StatusAssinatura : EntidadeDominio
{
    private StatusAssinatura() { }

    public StatusAssinatura(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
