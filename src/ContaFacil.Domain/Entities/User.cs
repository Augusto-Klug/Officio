using ContaFacil.Domain.Common;

namespace ContaFacil.Domain.Entities;

public class User : AuditableEntity
{
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool Ativo { get; private set; } = true;

    private User() { }

    public static User Create(Guid tenantId, string nome, string email, string senhaHash, string role)
    {
        return new User
        {
            TenantId = tenantId,
            Nome = nome,
            Email = email,
            SenhaHash = senhaHash,
            Role = role
        };
    }

    public void UpdatePassword(string newHash) => SenhaHash = newHash;
}
