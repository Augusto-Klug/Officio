using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IGestaoAcessoServico
{
    Task<Resultado<ParceiroOnboardingDto>> CriarParceiroAdministradorAsync(CriarParceiroAdministradorRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<List<ParceiroPlataformaDto>>> ListarParceirosAsync(CancellationToken ct = default);
    Task<Resultado<bool>> AlterarStatusParceiroAsync(Guid parceiroId, bool ativo, CancellationToken ct = default);
    Task<Resultado<ConviteCriadoDto>> ReenviarConviteAdministradorAsync(Guid parceiroId, CancellationToken ct = default);
    Task<Resultado<List<UsuarioGestaoDto>>> ListarUsuariosAsync(CancellationToken ct = default);
    Task<Resultado<List<ConviteUsuarioDto>>> ListarConvitesAsync(CancellationToken ct = default);
    Task<Resultado<ConviteCriadoDto>> ConvidarUsuarioAsync(CriarConviteUsuarioRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<ConviteCriadoDto>> ReenviarConviteAsync(Guid conviteId, CancellationToken ct = default);
    Task<Resultado<bool>> CancelarConviteAsync(Guid conviteId, CancellationToken ct = default);
    Task<Resultado<bool>> AceitarConviteAsync(AceitarConviteRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> AlterarPerfilAsync(Guid usuarioId, string perfil, CancellationToken ct = default);
    Task<Resultado<bool>> AlterarStatusAsync(Guid usuarioId, bool ativo, CancellationToken ct = default);
    Task SolicitarRedefinicaoSenhaAsync(string email, CancellationToken ct = default);
    Task<Resultado<bool>> RedefinirSenhaAsync(RedefinirSenhaRequisicao requisicao, CancellationToken ct = default);
}
