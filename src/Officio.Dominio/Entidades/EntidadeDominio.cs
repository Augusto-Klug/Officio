using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public abstract class EntidadeDominio : EntidadeBase
{
    public string Codigo { get; private set; } = default!;
    public string Descricao { get; private set; } = default!;
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; } = true;

    protected EntidadeDominio() { }

    protected EntidadeDominio(string codigo, string descricao, int ordem = 0)
    {
        Codigo = codigo;
        Descricao = descricao;
        Ordem = ordem;
    }

    public void Desativar() => Ativo = false;
}
