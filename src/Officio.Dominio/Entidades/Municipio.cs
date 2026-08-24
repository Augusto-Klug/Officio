using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Municipio : EntidadeBase
{
    public string CodigoIbge { get; private set; } = default!;
    public string Nome { get; private set; } = default!;
    public string Uf { get; private set; } = default!;
    public bool Ativo { get; private set; } = true;

    private Municipio() { }

    public static Municipio Criar(string codigoIbge, string nome, string uf)
        => new() { CodigoIbge = codigoIbge, Nome = nome, Uf = uf };
}
