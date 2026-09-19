namespace Officio.ViewModels.Empresas;

public record CriarEmpresaAdministradorViewModel(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string? EmailEmpresa,
    string NomeAdministrador,
    string EmailAdministrador);
