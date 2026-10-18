namespace Officio.Dominio.Contratos.Dtos;

public record ClienteDto(
    Guid Id,
    Guid EmpresaId,
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    string? RegimeTributario,
    string? SciCodigo,
    bool Ativo,
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacoes,
    Guid? ProspectOrigemId,
    int QuantidadeSocios,
    int QuantidadeContratos,
    int QuantidadeDocumentos);

public record ClienteDetalheDto(
    Guid Id,
    Guid EmpresaId,
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    string? RegimeTributario,
    string? SciCodigo,
    bool Ativo,
    string? Email,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacoes,
    Guid? ProspectOrigemId,
    DateTime? InicioRelacionamentoEm,
    IReadOnlyCollection<ClienteSocioDto> Socios,
    IReadOnlyCollection<ClienteContratoDto> Contratos,
    IReadOnlyCollection<ClienteDocumentoDto> Documentos);

public record ClienteSocioDto(
    Guid Id,
    string Nome,
    string? Cpf,
    decimal Participacao);

public record ClienteContratoDto(
    Guid Id,
    string Numero,
    string Status,
    decimal ValorTotal,
    DateTime DataInicio,
    DateTime? DataFim);

public record ClienteDocumentoDto(
    Guid Id,
    string NomeArquivo,
    string Tipo,
    bool Legivel,
    decimal QualidadeEstimada);
