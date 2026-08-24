namespace Officio.Dominio.Entidades;

public class StatusProspect : EntidadeDominio
{
    private StatusProspect() { }

    public StatusProspect(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
