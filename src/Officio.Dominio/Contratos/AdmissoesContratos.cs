using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Contratos;

public record DependenteAdmissaoRequisicao(
    string Nome,
    string? Cpf,
    DateOnly? DataNascimento,
    string? Parentesco,
    bool? DependenteIr,
    bool? SalarioFamilia);

public record DadosAdmissaoRequisicao(
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
    IReadOnlyCollection<DependenteAdmissaoRequisicao> Dependentes);

public record CriarAdmissaoManualRequisicao(Guid ClienteId, DadosAdmissaoRequisicao Dados);

public record AtualizarAdmissaoRequisicao(Guid AdmissaoId, DadosAdmissaoRequisicao Dados);
