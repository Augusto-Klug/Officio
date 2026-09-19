using Officio.Dominio.Comum;

namespace Officio.Dominio.Entidades;

public class TokenRedefinicaoSenha : EntidadeAuditavel
{
    public Guid UsuarioId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiraEm { get; private set; }
    public DateTime? UsadoEm { get; private set; }
    public DateTime? CanceladoEm { get; private set; }
    public Usuario Usuario { get; private set; } = default!;

    private TokenRedefinicaoSenha() { }

    public static TokenRedefinicaoSenha Criar(
        Guid tenantId,
        Guid usuarioId,
        string tokenHash,
        DateTime expiraEm)
        => new()
        {
            TenantId = tenantId,
            UsuarioId = usuarioId,
            TokenHash = tokenHash,
            ExpiraEm = expiraEm
        };

    public bool EstaValido(DateTime agora)
        => UsadoEm is null && CanceladoEm is null && ExpiraEm > agora;

    public void MarcarComoUsado(DateTime agora) => UsadoEm = agora;

    public void Cancelar(DateTime agora) => CanceladoEm ??= agora;
}
