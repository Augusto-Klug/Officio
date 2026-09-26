namespace Officio.Dominio.Contratos;

public record CriarParceiroAdministradorRequisicao(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string? EmailParceiro,
    string NomeAdministrador,
    string EmailAdministrador);

public record CriarConviteUsuarioRequisicao(string Nome, string Email, string Perfil);

public record AceitarConviteRequisicao(string Token, string Senha);

public record RedefinirSenhaRequisicao(string Token, string Senha);

public record ParceiroOnboardingDto(Guid ParceiroId, Guid ConviteId, string EmailAdministrador, DateTime ExpiraEm);

public record ParceiroPlataformaDto(
    Guid Id,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    bool Ativo,
    DateTime CriadaEm,
    int QuantidadeUsuarios,
    string? AdministradorEmail);

public record UsuarioGestaoDto(Guid Id, string Nome, string Email, string Perfil, bool Ativo, DateTime CriadoEm);

public record ConviteUsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    string Perfil,
    DateTime ExpiraEm,
    string Status,
    DateTime CriadoEm);

public record ConviteCriadoDto(Guid Id, string Email, DateTime ExpiraEm);
