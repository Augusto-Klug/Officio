using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Contratos;

public record GerarLinkFichaAdmissaoDto(string Token, DateTime ExpiraEm);

public record AtualizarFichaAdmissaoRequisicao(
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
    IReadOnlyCollection<DependenteAdmissaoRequisicao> Dependentes);

public record FichaAdmissaoPublicaDto(
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
    bool? UtilizaValeTransporte,
    DateOnly? DataAdmissaoPrevista,
    string? Funcao,
    bool PreenchimentoConcluido,
    DateTime ExpiraEm,
    IReadOnlyCollection<AdmissaoDependenteDto> Dependentes,
    IReadOnlyCollection<AdmissaoDocumentoDto> Documentos);
