using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class Usuario : EntidadeAuditavel
{
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool Ativo { get; private set; } = true;

    private Usuario() { }

    public static Usuario Create(Guid tenantId, string nome, string email, string senhaHash, string role)
    {
        return new Usuario
        {
            TenantId = tenantId,
            Nome = nome,
            Email = email,
            SenhaHash = senhaHash,
            Role = role
        };
    }

    public void UpdatePassword(string newHash) => SenhaHash = newHash;

    public void AlterarPerfil(string perfil) => Role = perfil;

    public void AlterarStatus(bool ativo) => Ativo = ativo;
}





