namespace Officio.Dominio.Entidades;

public class StatusFaturamento : EntidadeDominio
{
    private StatusFaturamento() { }

    public StatusFaturamento(string codigo, string descricao)
        : base(codigo, descricao) { }
}
