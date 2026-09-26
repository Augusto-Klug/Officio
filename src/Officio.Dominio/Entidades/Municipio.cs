using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Municipio : EntidadeBase
{
    public Guid EstadoId { get; private set; }
    public string CodigoIbge { get; private set; } = default!;
    public string Nome { get; private set; } = default!;
    public bool Ativo { get; private set; } = true;
    public Estado Estado { get; private set; } = default!;

    private Municipio() { }

    public static Municipio Criar(Guid estadoId, string codigoIbge, string nome)
        => new() { EstadoId = estadoId, CodigoIbge = codigoIbge, Nome = nome.Trim() };

    public void Atualizar(Guid estadoId, string nome)
    {
        EstadoId = estadoId;
        Nome = nome.Trim();
        Ativo = true;
    }
}
