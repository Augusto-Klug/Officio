namespace Officio.ViewModels.Admissoes;

public record AtualizarFichaAdmissaoViewModel(
    string NomeFuncionario,
    string? Cpf,
    string? Rg,
    DateOnly? DataNascimento,
    string? Sexo,
    string? EstadoCivil,
    string? Escolaridade,
    string? Telefone,
    string? Email,
    string? Pis,
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta,
    bool? UtilizaValeTransporte,
    IReadOnlyCollection<AdmissaoDependenteViewModel>? Dependentes);
