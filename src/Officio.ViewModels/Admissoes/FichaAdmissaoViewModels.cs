namespace Officio.ViewModels.Admissoes;

public record AtualizarFichaFuncionarioViewModel(
    string NomeFuncionario,
    DateOnly? DataNascimento,
    string? Sexo,
    string? EstadoCivil,
    string? Escolaridade,
    string? Telefone,
    string? Email,
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta,
    IReadOnlyCollection<AdmissaoDependenteViewModel>? Dependentes,
    string? Cpf = null,
    string? Rg = null,
    string? Etnia = null);

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
    string? HorarioTrabalho);
