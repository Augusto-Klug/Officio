using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class ConviteUsuario : EntidadeAuditavel
{
    public string Nome { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Perfil { get; private set; } = default!;
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiraEm { get; private set; }
    public DateTime? AceitoEm { get; private set; }
    public DateTime? CanceladoEm { get; private set; }

    private ConviteUsuario() { }

    public static ConviteUsuario Criar(
        Guid tenantId,
        string nome,
        string email,
        string perfil,
        string tokenHash,
        DateTime expiraEm)
        => new()
        {
            TenantId = tenantId,
            Nome = nome,
            Email = email,
            Perfil = perfil,
            TokenHash = tokenHash,
            ExpiraEm = expiraEm
        };

    public bool EstaPendente(DateTime agora)
        => AceitoEm is null && CanceladoEm is null && ExpiraEm > agora;

    public void Aceitar(DateTime agora) => AceitoEm = agora;

    public void Cancelar(DateTime agora) => CanceladoEm = agora;

    public void Reenviar(string tokenHash, DateTime expiraEm)
    {
        TokenHash = tokenHash;
        ExpiraEm = expiraEm;
        CanceladoEm = null;
    }
}
