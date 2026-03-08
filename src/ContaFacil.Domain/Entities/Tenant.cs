using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Nome { get; private set; } = default!;
    public string? Cnpj { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Tenant() { }

    public static Tenant Create(string nome, string? cnpj)
    {
        return new Tenant
        {
            Nome = nome,
            Cnpj = cnpj
        };
    }
}
