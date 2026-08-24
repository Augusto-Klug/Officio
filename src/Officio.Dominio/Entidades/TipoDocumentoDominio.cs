namespace Officio.Dominio.Entidades;

public class TipoDocumentoDominio : EntidadeDominio
{
    private TipoDocumentoDominio() { }

    public TipoDocumentoDominio(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
