namespace Officio.Dominio.Contratos.Dtos;

public record EmpresaAdmissoesResumoDto(
    Guid EmpresaId,
    string EmpresaNome,
    string? Cnpj,
    int QuantidadeAdmissoes);

public record AdmissaoResumoDto(
    Guid Id,
    Guid EmpresaId,
    string EmpresaNome,
    string NomeFuncionario,
    string? Cpf,
    string Status,
    string StatusDescricao,
    DateOnly? DataAdmissaoPrevista,
    string? Funcao,
    decimal? Salario,
    DateTime CriadoEm);

public record AdmissaoDependenteDto(
    Guid Id,
    string Nome,
    string? Cpf,
    DateOnly? DataNascimento,
    string? Parentesco,
    bool? DependenteIr,
    bool? SalarioFamilia);

public record AdmissaoDetalheDto(
    Guid Id,
    Guid EmpresaId,
    string EmpresaNome,
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
    string Status,
    string StatusDescricao,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    IReadOnlyCollection<AdmissaoDependenteDto> Dependentes);
