using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Seguranca;

namespace Officio.Dominio.Servicos.Servicos;

public class GestaoAcessoServico(
    IGestaoAcessoRepositorio repositorio,
    IEmpresaAtual empresaAtual,
    IUsuarioAtual usuarioAtual,
    IServicoSenha servicoSenha,
    IServicoEmail servicoEmail,
    IConfiguration configuracao) : IGestaoAcessoServico
{
    public async Task<Resultado<EmpresaOnboardingDto>> CriarEmpresaAdministradorAsync(
        CriarEmpresaAdministradorRequisicao requisicao,
        CancellationToken ct = default)
    {
        var cnpj = SomenteDigitos(requisicao.Cnpj);
        var email = NormalizarEmail(requisicao.EmailAdministrador);
        if (await repositorio.EmpresaExistePorCnpjAsync(cnpj, ct))
            return Resultado<EmpresaOnboardingDto>.CriarFalha("Ja existe uma empresa com este CNPJ.");
        if (await repositorio.UsuarioExistePorEmailAsync(email, ct))
            return Resultado<EmpresaOnboardingDto>.CriarFalha("Ja existe um usuario com este e-mail.");

        var empresa = Empresa.CriarConstituida(
            cnpj,
            requisicao.RazaoSocial.Trim(),
            LimparOpcional(requisicao.NomeFantasia),
            email: LimparOpcional(requisicao.EmailEmpresa));
        var token = GerarToken();
        var expiraEm = DateTime.UtcNow.AddHours(ObterHoras("Acesso:ConviteExpiracaoHoras", 48));
        var convite = ConviteUsuario.Criar(
            empresa.Id,
            requisicao.NomeAdministrador.Trim(),
            email,
            PerfisAcesso.Admin,
            GerarHashToken(token),
            expiraEm);

        await repositorio.AdicionarAsync(empresa, ct);
        await repositorio.AdicionarAsync(convite, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        await EnviarConviteAsync(convite, token, ct);

        return Resultado<EmpresaOnboardingDto>.CriarSucesso(
            new EmpresaOnboardingDto(empresa.Id, convite.Id, convite.Email, convite.ExpiraEm));
    }

    public async Task<Resultado<List<UsuarioGestaoDto>>> ListarUsuariosAsync(CancellationToken ct = default)
    {
        var usuarios = await repositorio.ListarUsuariosAsync(ct);
        return Resultado<List<UsuarioGestaoDto>>.CriarSucesso(usuarios
            .OrderBy(usuario => usuario.Nome)
            .Select(usuario => new UsuarioGestaoDto(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Role,
                usuario.Ativo,
                usuario.CriadoEm))
            .ToList());
    }

    public async Task<Resultado<List<EmpresaPlataformaDto>>> ListarEmpresasAsync(CancellationToken ct = default)
    {
        var empresas = await repositorio.ListarEmpresasAsync(ct);
        var resultado = new List<EmpresaPlataformaDto>(empresas.Count);

        foreach (var empresa in empresas)
        {
            var convite = await repositorio.ObterConviteAdministradorAsync(empresa.Id, ct);
            resultado.Add(new EmpresaPlataformaDto(
                empresa.Id,
                empresa.RazaoSocial,
                empresa.NomeFantasia,
                empresa.Cnpj,
                empresa.Ativo,
                empresa.CriadoEm,
                await repositorio.ContarUsuariosAsync(empresa.Id, ct),
                convite?.Email));
        }

        return Resultado<List<EmpresaPlataformaDto>>.CriarSucesso(resultado);
    }

    public async Task<Resultado<bool>> AlterarStatusEmpresaAsync(
        Guid empresaId,
        bool ativa,
        CancellationToken ct = default)
    {
        var empresa = await repositorio.ObterEmpresaAsync(empresaId, ct);
        if (empresa is null)
            return Resultado<bool>.CriarFalha("Empresa nao encontrada.");

        empresa.AlterarStatus(ativa);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<ConviteCriadoDto>> ReenviarConviteAdministradorAsync(
        Guid empresaId,
        CancellationToken ct = default)
    {
        if (await repositorio.ObterEmpresaAsync(empresaId, ct) is null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Empresa nao encontrada.");

        var convite = await repositorio.ObterConviteAdministradorAsync(empresaId, ct);
        if (convite is null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Convite do administrador nao encontrado.");
        if (convite.AceitoEm is not null || await repositorio.UsuarioExistePorEmailAsync(convite.Email, ct))
            return Resultado<ConviteCriadoDto>.CriarFalha("O administrador ja ativou a conta.");

        var token = GerarToken();
        convite.Reenviar(
            GerarHashToken(token),
            DateTime.UtcNow.AddHours(ObterHoras("Acesso:ConviteExpiracaoHoras", 48)));
        await repositorio.SalvarAlteracoesAsync(ct);
        await EnviarConviteAsync(convite, token, ct);
        return Resultado<ConviteCriadoDto>.CriarSucesso(new(convite.Id, convite.Email, convite.ExpiraEm));
    }

    public async Task<Resultado<List<ConviteUsuarioDto>>> ListarConvitesAsync(CancellationToken ct = default)
    {
        var agora = DateTime.UtcNow;
        var convites = await repositorio.ListarConvitesAsync(ct);
        return Resultado<List<ConviteUsuarioDto>>.CriarSucesso(convites
            .OrderByDescending(convite => convite.CriadoEm)
            .Select(convite => new ConviteUsuarioDto(
                convite.Id,
                convite.Nome,
                convite.Email,
                convite.Perfil,
                convite.ExpiraEm,
                ObterStatus(convite, agora),
                convite.CriadoEm))
            .ToList());
    }

    public async Task<Resultado<ConviteCriadoDto>> ConvidarUsuarioAsync(
        CriarConviteUsuarioRequisicao requisicao,
        CancellationToken ct = default)
    {
        var perfil = PerfisAcesso.NormalizarPerfilTenant(requisicao.Perfil);
        if (perfil is null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Perfil invalido para usuario do tenant.");

        var email = NormalizarEmail(requisicao.Email);
        if (await repositorio.UsuarioExistePorEmailAsync(email, ct))
            return Resultado<ConviteCriadoDto>.CriarFalha("Ja existe um usuario com este e-mail.");
        if (await repositorio.ObterConvitePendentePorEmailAsync(empresaAtual.TenantId, email, ct) is not null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Ja existe um convite pendente para este e-mail.");

        var token = GerarToken();
        var convite = ConviteUsuario.Criar(
            empresaAtual.TenantId,
            requisicao.Nome.Trim(),
            email,
            perfil,
            GerarHashToken(token),
            DateTime.UtcNow.AddHours(ObterHoras("Acesso:ConviteExpiracaoHoras", 48)));

        await repositorio.AdicionarAsync(convite, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        await EnviarConviteAsync(convite, token, ct);
        return Resultado<ConviteCriadoDto>.CriarSucesso(new(convite.Id, convite.Email, convite.ExpiraEm));
    }

    public async Task<Resultado<ConviteCriadoDto>> ReenviarConviteAsync(Guid conviteId, CancellationToken ct = default)
    {
        var convite = await repositorio.ObterConviteAsync(conviteId, ct);
        if (convite is null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Convite nao encontrado.");
        if (convite.AceitoEm is not null)
            return Resultado<ConviteCriadoDto>.CriarFalha("Convite ja foi aceito.");
        if (await repositorio.UsuarioExistePorEmailAsync(convite.Email, ct))
            return Resultado<ConviteCriadoDto>.CriarFalha("Ja existe um usuario com este e-mail.");

        var token = GerarToken();
        convite.Reenviar(
            GerarHashToken(token),
            DateTime.UtcNow.AddHours(ObterHoras("Acesso:ConviteExpiracaoHoras", 48)));
        await repositorio.SalvarAlteracoesAsync(ct);
        await EnviarConviteAsync(convite, token, ct);
        return Resultado<ConviteCriadoDto>.CriarSucesso(new(convite.Id, convite.Email, convite.ExpiraEm));
    }

    public async Task<Resultado<bool>> CancelarConviteAsync(Guid conviteId, CancellationToken ct = default)
    {
        var convite = await repositorio.ObterConviteAsync(conviteId, ct);
        if (convite is null)
            return Resultado<bool>.CriarFalha("Convite nao encontrado.");
        if (convite.AceitoEm is not null)
            return Resultado<bool>.CriarFalha("Convite ja foi aceito.");

        convite.Cancelar(DateTime.UtcNow);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> AceitarConviteAsync(AceitarConviteRequisicao requisicao, CancellationToken ct = default)
    {
        var convite = await repositorio.ObterConvitePorTokenAsync(GerarHashToken(requisicao.Token), ct);
        var agora = DateTime.UtcNow;
        if (convite is null || !convite.EstaPendente(agora))
            return Resultado<bool>.CriarFalha("Convite invalido ou expirado.");
        if (await repositorio.UsuarioExistePorEmailAsync(convite.Email, ct))
            return Resultado<bool>.CriarFalha("Este convite nao pode mais ser utilizado.");

        var usuario = Usuario.Create(convite.TenantId, convite.Nome, convite.Email, string.Empty, convite.Perfil);
        usuario.UpdatePassword(servicoSenha.GerarHash(usuario, requisicao.Senha));
        var vinculo = UsuarioEmpresa.Criar(usuario.Id, convite.TenantId);
        convite.Aceitar(agora);

        await repositorio.AdicionarAsync(usuario, ct);
        await repositorio.AdicionarAsync(vinculo, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> AlterarPerfilAsync(Guid usuarioId, string perfil, CancellationToken ct = default)
    {
        var perfilNormalizado = PerfisAcesso.NormalizarPerfilTenant(perfil);
        if (perfilNormalizado is null)
            return Resultado<bool>.CriarFalha("Perfil invalido para usuario do tenant.");
        var usuario = await repositorio.ObterUsuarioAsync(usuarioId, ct);
        if (usuario is null)
            return Resultado<bool>.CriarFalha("Usuario nao encontrado.");
        if (usuario.Role == PerfisAcesso.Admin && perfilNormalizado != PerfisAcesso.Admin &&
            await repositorio.ContarAdministradoresAtivosAsync(empresaAtual.TenantId, ct) <= 1)
            return Resultado<bool>.CriarFalha("A empresa deve manter ao menos um administrador ativo.");

        usuario.AlterarPerfil(perfilNormalizado);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> AlterarStatusAsync(Guid usuarioId, bool ativo, CancellationToken ct = default)
    {
        var usuario = await repositorio.ObterUsuarioAsync(usuarioId, ct);
        if (usuario is null)
            return Resultado<bool>.CriarFalha("Usuario nao encontrado.");
        if (!ativo && usuario.Id == usuarioAtual.UserId)
            return Resultado<bool>.CriarFalha("Nao e permitido desativar o proprio usuario.");
        if (!ativo && usuario.Role == PerfisAcesso.Admin &&
            await repositorio.ContarAdministradoresAtivosAsync(empresaAtual.TenantId, ct) <= 1)
            return Resultado<bool>.CriarFalha("A empresa deve manter ao menos um administrador ativo.");

        usuario.AlterarStatus(ativo);
        var vinculo = await repositorio.ObterVinculoAsync(usuario.Id, empresaAtual.TenantId, ct);
        vinculo?.AlterarStatus(ativo);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task SolicitarRedefinicaoSenhaAsync(string email, CancellationToken ct = default)
    {
        var usuario = await repositorio.ObterUsuarioPorEmailAsync(NormalizarEmail(email), ct);
        if (usuario is null || !usuario.Ativo)
            return;

        var agora = DateTime.UtcNow;
        foreach (var tokenAnterior in await repositorio.ListarTokensRedefinicaoAtivosAsync(usuario.Id, ct))
            tokenAnterior.Cancelar(agora);

        var token = GerarToken();
        var redefinicao = TokenRedefinicaoSenha.Criar(
            usuario.TenantId,
            usuario.Id,
            GerarHashToken(token),
            agora.AddHours(ObterHoras("Acesso:RedefinicaoSenhaExpiracaoHoras", 1)));
        await repositorio.AdicionarAsync(redefinicao, ct);
        await repositorio.SalvarAlteracoesAsync(ct);

        var link = $"{ObterBaseFrontend()}/redefinir-senha?token={Uri.EscapeDataString(token)}";
        await servicoEmail.SendAsync(
            usuario.Email,
            "Redefinicao de senha do Officio",
            $"Use o link a seguir para definir uma nova senha: {link}",
            ct);
    }

    public async Task<Resultado<bool>> RedefinirSenhaAsync(RedefinirSenhaRequisicao requisicao, CancellationToken ct = default)
    {
        var token = await repositorio.ObterTokenRedefinicaoAsync(GerarHashToken(requisicao.Token), ct);
        var agora = DateTime.UtcNow;
        if (token is null || !token.EstaValido(agora))
            return Resultado<bool>.CriarFalha("Token de redefinicao invalido ou expirado.");

        var usuario = await repositorio.ObterUsuarioPorEmailAsync(token.Usuario.Email, ct);
        if (usuario is null || !usuario.Ativo || usuario.Id != token.UsuarioId)
            return Resultado<bool>.CriarFalha("Token de redefinicao invalido ou expirado.");

        usuario.UpdatePassword(servicoSenha.GerarHash(usuario, requisicao.Senha));
        token.MarcarComoUsado(agora);
        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task EnviarConviteAsync(ConviteUsuario convite, string token, CancellationToken ct)
    {
        var link = $"{ObterBaseFrontend()}/ativar-conta?token={Uri.EscapeDataString(token)}";
        await servicoEmail.SendAsync(
            convite.Email,
            "Convite para acessar o Officio",
            $"Ola, {convite.Nome}. Defina sua senha e ative a conta em: {link}",
            ct);
    }

    private string ObterBaseFrontend()
        => (configuracao["Acesso:FrontendBaseUrl"] ?? "http://localhost:5173").TrimEnd('/');

    private double ObterHoras(string chave, double padrao)
    {
        var horas = double.TryParse(
            configuracao[chave],
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var configurado)
            ? configurado
            : padrao;
        return double.IsFinite(horas) && horas > 0 ? horas : padrao;
    }

    private static string ObterStatus(ConviteUsuario convite, DateTime agora)
        => convite.AceitoEm is not null ? "Aceito"
            : convite.CanceladoEm is not null ? "Cancelado"
            : convite.ExpiraEm <= agora ? "Expirado"
            : "Pendente";

    private static string GerarToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string GerarHashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
    private static string SomenteDigitos(string valor) => new(valor.Where(char.IsDigit).ToArray());
    private static string? LimparOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
