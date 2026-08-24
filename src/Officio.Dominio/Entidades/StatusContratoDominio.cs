namespace Officio.Dominio.Entidades;

public class StatusContratoDominio : EntidadeDominio
{
    private StatusContratoDominio() { }

    public StatusContratoDominio(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
