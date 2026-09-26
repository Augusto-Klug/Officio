using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IGestaoAcessoRepositorio
{
    Task<bool> ParceiroExistePorCnpjAsync(string cnpj, CancellationToken ct = default);
    Task<List<Parceiro>> ListarParceirosAsync(CancellationToken ct = default);
    Task<Parceiro?> ObterParceiroAsync(Guid parceiroId, CancellationToken ct = default);
    Task<int> ContarUsuariosAsync(Guid tenantId, CancellationToken ct = default);
    Task<ConviteUsuario?> ObterConviteAdministradorAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> UsuarioExistePorEmailAsync(string email, CancellationToken ct = default);
    Task<ConviteUsuario?> ObterConvitePendentePorEmailAsync(Guid tenantId, string email, CancellationToken ct = default);
    Task<ConviteUsuario?> ObterConviteAsync(Guid conviteId, CancellationToken ct = default);
    Task<ConviteUsuario?> ObterConvitePorTokenAsync(string tokenHash, CancellationToken ct = default);
    Task<List<ConviteUsuario>> ListarConvitesAsync(CancellationToken ct = default);
    Task<List<Usuario>> ListarUsuariosAsync(CancellationToken ct = default);
    Task<Usuario?> ObterUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<UsuarioEmpresa?> ObterVinculoAsync(Guid usuarioId, Guid empresaId, CancellationToken ct = default);
    Task<int> ContarAdministradoresAtivosAsync(Guid tenantId, CancellationToken ct = default);
    Task<Usuario?> ObterUsuarioPorEmailAsync(string email, CancellationToken ct = default);
    Task<TokenRedefinicaoSenha?> ObterTokenRedefinicaoAsync(string tokenHash, CancellationToken ct = default);
    Task<List<TokenRedefinicaoSenha>> ListarTokensRedefinicaoAtivosAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync<T>(T entidade, CancellationToken ct = default) where T : class;
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
