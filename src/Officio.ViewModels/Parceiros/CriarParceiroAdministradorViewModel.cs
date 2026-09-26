namespace Officio.ViewModels.Parceiros;

public record CriarParceiroAdministradorViewModel(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string? EmailParceiro,
    string NomeAdministrador,
    string EmailAdministrador);
