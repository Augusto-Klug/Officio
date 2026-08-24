namespace Officio.Dominio.Entidades;

public class SecaoOrcamento : EntidadeDominio
{
    private SecaoOrcamento() { }

    public SecaoOrcamento(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
