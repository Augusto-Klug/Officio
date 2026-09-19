namespace Officio.ViewModels.Admissoes;

public record AdmissaoDependenteViewModel(
    string Nome,
    string? Cpf,
    DateOnly? DataNascimento,
    string? Parentesco,
    bool? DependenteIr,
    bool? SalarioFamilia);

public record DadosAdmissaoViewModel(
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
    DateOnly? DataAdmissaoPrevista,
    decimal? Salario,
    string? Comissao,
    string? Funcao,
    int? ContratoExperienciaDias,
    bool? UtilizaValeTransporte,
    string? HorarioTrabalho,
    string? Observacoes,
    IReadOnlyCollection<AdmissaoDependenteViewModel>? Dependentes);

public record CriarAdmissaoManualViewModel(Guid ClienteId, DadosAdmissaoViewModel Dados);

public record AtualizarAdmissaoViewModel(DadosAdmissaoViewModel Dados);
