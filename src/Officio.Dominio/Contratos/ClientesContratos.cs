using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Contratos;

public record AtualizarClienteRequisicao(
    Guid ClienteId,
    string RazaoSocial,
    string? NomeFantasia,
    string? Cnpj,
    string? Cpf,
    RegimeTributario? RegimeTributario,
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
    string? InscricaoEstadual = null,
    string? InscricaoMunicipal = null,
    string? TipoEmpresa = null,
    string? MunicipioCodigoIbge = null,
    IReadOnlyCollection<CriarSocioRequisicao>? Socios = null,
    bool ConfirmarCancelamentoContratos = false,
    IReadOnlyCollection<Guid>? ContratosConfirmados = null,
    string? Complemento = null);

public record VincularCodigoSciRequisicao(Guid ClienteId, string SciCodigo);

public record ClienteResultado(
    Guid Id,
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

public record ClienteDetalheResultado(
    Guid Id,
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
    IReadOnlyCollection<ClienteSocioResultado> Socios,
    IReadOnlyCollection<ClienteContratoResultado> Contratos,
    IReadOnlyCollection<ClienteDocumentoResultado> Documentos);

public record ClienteSocioResultado(
    Guid Id, string Nome, string? Cpf, decimal Participacao, bool Ativo,
    DateTime VigenteDesde, DateTime? VigenteAte, string? Profissao, Guid? TipoRelacaoId,
    string? TipoRelacao, string? Cep, string? Logradouro, string? Numero, string? Complemento,
    string? Bairro, string? Cidade, string? Uf, string? MunicipioCodigoIbge);

public record ClienteContratoResultado(
    Guid Id,
    string Numero,
    string Status,
    decimal ValorTotal,
    DateTime DataInicio,
    DateTime? DataFim);

public record ClienteDocumentoResultado(
    Guid Id,
    string NomeArquivo,
    string Tipo,
    bool Legivel,
    decimal QualidadeEstimada);
