using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Estado : EntidadeBase
{
    public string CodigoIbge { get; private set; } = default!;
    public string Sigla { get; private set; } = default!;
    public string Nome { get; private set; } = default!;
    public bool Ativo { get; private set; } = true;
    public DateTime? MunicipiosSincronizadosEm { get; private set; }
    public ICollection<Municipio> Municipios { get; private set; } = [];

    private Estado() { }

    public static Estado Criar(string codigoIbge, string sigla, string nome)
        => new()
        {
            CodigoIbge = codigoIbge,
            Sigla = sigla.Trim().ToUpperInvariant(),
            Nome = nome.Trim()
        };

    public void Atualizar(string codigoIbge, string nome)
    {
        CodigoIbge = codigoIbge;
        Nome = nome.Trim();
        Ativo = true;
    }

    public void MarcarMunicipiosSincronizados(DateTime instante)
        => MunicipiosSincronizadosEm = instante;
}
