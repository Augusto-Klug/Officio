using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

public class GestaoAcessoRepositorio(ContextoAplicacao contexto) : IGestaoAcessoRepositorio
{
    public Task<bool> ParceiroExistePorCnpjAsync(string cnpj, CancellationToken ct = default)
        => contexto.Parceiros.AnyAsync(parceiro => parceiro.Cnpj == cnpj, ct);

    public Task<List<Parceiro>> ListarParceirosAsync(CancellationToken ct = default)
        => contexto.Parceiros.AsNoTracking().OrderBy(parceiro => parceiro.RazaoSocial).ToListAsync(ct);

    public Task<Parceiro?> ObterParceiroAsync(Guid parceiroId, CancellationToken ct = default)
        => contexto.Parceiros.FirstOrDefaultAsync(parceiro => parceiro.Id == parceiroId, ct);

    public Task<int> ContarUsuariosAsync(Guid tenantId, CancellationToken ct = default)
        => contexto.Users.IgnoreQueryFilters().CountAsync(usuario => usuario.TenantId == tenantId, ct);

    public Task<ConviteUsuario?> ObterConviteAdministradorAsync(Guid tenantId, CancellationToken ct = default)
        => contexto.ConvitesUsuario.IgnoreQueryFilters()
            .Where(convite => convite.TenantId == tenantId && convite.Perfil == PerfisAcesso.Admin)
            .OrderByDescending(convite => convite.CriadoEm)
            .FirstOrDefaultAsync(ct);

    public Task<bool> UsuarioExistePorEmailAsync(string email, CancellationToken ct = default)
        => contexto.Users.IgnoreQueryFilters().AnyAsync(usuario => usuario.Email == email, ct);

    public Task<ConviteUsuario?> ObterConvitePendentePorEmailAsync(
        Guid tenantId,
        string email,
        CancellationToken ct = default)
        => contexto.ConvitesUsuario.FirstOrDefaultAsync(
            convite => convite.TenantId == tenantId &&
                       convite.Email == email &&
                       convite.AceitoEm == null &&
                       convite.CanceladoEm == null &&
                       convite.ExpiraEm > DateTime.UtcNow,
            ct);

    public Task<ConviteUsuario?> ObterConviteAsync(Guid conviteId, CancellationToken ct = default)
        => contexto.ConvitesUsuario.FirstOrDefaultAsync(convite => convite.Id == conviteId, ct);

    public Task<ConviteUsuario?> ObterConvitePorTokenAsync(string tokenHash, CancellationToken ct = default)
        => contexto.ConvitesUsuario.IgnoreQueryFilters()
            .FirstOrDefaultAsync(convite => convite.TokenHash == tokenHash, ct);

    public Task<List<ConviteUsuario>> ListarConvitesAsync(CancellationToken ct = default)
        => contexto.ConvitesUsuario.AsNoTracking().ToListAsync(ct);

    public Task<List<Usuario>> ListarUsuariosAsync(CancellationToken ct = default)
        => contexto.Users.AsNoTracking().ToListAsync(ct);

    public Task<Usuario?> ObterUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => contexto.Users.FirstOrDefaultAsync(usuario => usuario.Id == usuarioId, ct);

    public Task<UsuarioEmpresa?> ObterVinculoAsync(
        Guid usuarioId,
        Guid empresaId,
        CancellationToken ct = default)
        => contexto.UsuarioEmpresas.FirstOrDefaultAsync(
            vinculo => vinculo.UsuarioId == usuarioId && vinculo.EmpresaId == empresaId,
            ct);

    public Task<int> ContarAdministradoresAtivosAsync(Guid tenantId, CancellationToken ct = default)
        => contexto.Users.CountAsync(
            usuario => usuario.TenantId == tenantId && usuario.Ativo && usuario.Role == PerfisAcesso.Admin,
            ct);

    public Task<Usuario?> ObterUsuarioPorEmailAsync(string email, CancellationToken ct = default)
        => contexto.Users.IgnoreQueryFilters().FirstOrDefaultAsync(usuario => usuario.Email == email, ct);

    public Task<TokenRedefinicaoSenha?> ObterTokenRedefinicaoAsync(string tokenHash, CancellationToken ct = default)
        => contexto.TokensRedefinicaoSenha.IgnoreQueryFilters()
            .Include(token => token.Usuario)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

    public Task<List<TokenRedefinicaoSenha>> ListarTokensRedefinicaoAtivosAsync(
        Guid usuarioId,
        CancellationToken ct = default)
        => contexto.TokensRedefinicaoSenha.IgnoreQueryFilters()
            .Where(token => token.UsuarioId == usuarioId && token.UsadoEm == null && token.CanceladoEm == null)
            .ToListAsync(ct);

    public async Task AdicionarAsync<T>(T entidade, CancellationToken ct = default) where T : class
        => await contexto.Set<T>().AddAsync(entidade, ct);

    public async Task SalvarAlteracoesAsync(CancellationToken ct = default)
        => await contexto.SaveChangesAsync(ct);
}
