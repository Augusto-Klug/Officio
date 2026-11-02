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
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta,
    IReadOnlyCollection<DependenteAdmissaoRequisicao> Dependentes,
    string? Cpf = null,
    string? Rg = null,
    string? Etnia = null);

public record AtualizarFichaEmpresaRequisicao(
    string? Pis,
    DateOnly? DataAdmissaoPrevista,
    decimal? Salario,
    string? Comissao,
    string? Funcao,
    int? ContratoExperienciaDias,
    bool? UtilizaValeTransporte,
    string? HorarioTrabalho);

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
    string? Banco,
    string? Agencia,
    string? Conta,
    string? TipoConta,
    string? Etnia,
    bool PreenchimentoConcluido,
    DateTime ExpiraEm,
    IReadOnlyCollection<AdmissaoDependenteDto> Dependentes,
    IReadOnlyCollection<AdmissaoDocumentoDto> Documentos);
