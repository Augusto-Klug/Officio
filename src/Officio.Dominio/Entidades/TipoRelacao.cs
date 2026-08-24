namespace Officio.Dominio.Entidades;

public class TipoRelacao : EntidadeDominio
{
    public string Escopo { get; private set; } = default!;

    private TipoRelacao() { }

    public TipoRelacao(string codigo, string descricao, string escopo)
        : base(codigo, descricao)
        => Escopo = escopo;
}
