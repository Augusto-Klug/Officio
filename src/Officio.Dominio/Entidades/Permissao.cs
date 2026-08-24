namespace Officio.Dominio.Entidades;

public class Permissao : EntidadeDominio
{
    private Permissao() { }

    public Permissao(string codigo, string descricao)
        : base(codigo, descricao) { }
}
