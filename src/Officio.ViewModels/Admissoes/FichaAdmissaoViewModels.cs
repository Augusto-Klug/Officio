namespace Officio.ViewModels.Admissoes;

public record AtualizarFichaFuncionarioViewModel(
    string NomeFuncionario,
    DateOnly? DataNascimento,
    string? Sexo,
    string? EstadoCivil,
    string? Escolaridade,
    string? Telefone,
    string? Email,
    IReadOnlyCollection<AdmissaoDependenteViewModel>? Dependentes);

public record EnviarLinkFichaAdmissaoEmailViewModel(
    string Papel,
    string Email,
    string Token);

public record AtualizarFichaEmpresaViewModel(
    string? Pis,
    DateOnly? DataAdmissaoPrevista,
    decimal? Salario,
    string? Comissao,
    string? Funcao,
    int? ContratoExperienciaDias,
    bool? UtilizaValeTransporte,
    string? HorarioTrabalho,
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta);
