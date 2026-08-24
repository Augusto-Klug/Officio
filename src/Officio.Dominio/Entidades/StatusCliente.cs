namespace Officio.Dominio.Entidades;

public class StatusCliente : EntidadeDominio
{
    private StatusCliente() { }

    public StatusCliente(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
