using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Contratos;

public record GerarLinkFichaAdmissaoDto(string Token, DateTime ExpiraEm);

public record EnviarLinkFichaAdmissaoEmailRequisicao(
    Guid AdmissaoId,
    string Papel,
    string Email,
    string Token);

public record AtualizarFichaFuncionarioRequisicao(
    string NomeFuncionario,
    DateOnly? DataNascimento,
    string? Sexo,
    string? EstadoCivil,
    string? Escolaridade,
    string? Telefone,
    string? Email,
    IReadOnlyCollection<DependenteAdmissaoRequisicao> Dependentes);

public record AtualizarFichaEmpresaRequisicao(
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

public record FichaFuncionarioPublicaDto(
    Guid Id,
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
    bool PreenchimentoConcluido,
    DateTime ExpiraEm,
    IReadOnlyCollection<AdmissaoDependenteDto> Dependentes,
    IReadOnlyCollection<AdmissaoDocumentoDto> Documentos);

public record FichaEmpresaPublicaDto(
    Guid Id,
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
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta,
    string? Pis,
    DateOnly? DataAdmissaoPrevista,
    decimal? Salario,
    string? Comissao,
    string? Funcao,
    int? ContratoExperienciaDias,
    bool? UtilizaValeTransporte,
    string? HorarioTrabalho,
    bool PreenchimentoConcluido,
    DateTime ExpiraEm);
