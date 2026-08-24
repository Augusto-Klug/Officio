namespace Officio.Dominio.Entidades;

public class StatusOrcamentoDominio : EntidadeDominio
{
    private StatusOrcamentoDominio() { }

    public StatusOrcamentoDominio(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
