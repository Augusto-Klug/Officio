namespace Officio.Dominio.Entidades;

public class StatusAdmissao : EntidadeDominio
{
    private StatusAdmissao() { }

    public StatusAdmissao(string codigo, string descricao, int ordem)
        : base(codigo, descricao, ordem) { }
}
