using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Empresa : EntidadeBase
{
    public string Nome { get; private set; } = default!;
    public string? Cnpj { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Empresa() { }

    public static Empresa Create(string nome, string? cnpj)
    {
        return new Empresa
        {
            Nome = nome,
            Cnpj = cnpj
        };
    }
}





