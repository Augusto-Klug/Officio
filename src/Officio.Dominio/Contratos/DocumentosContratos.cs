using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Contratos;

public record EnviarDocumentoRequisicao(
    Stream Arquivo,
    string NomeArquivo,
    TipoDocumento Tipo,
    Guid? ClienteId,
    Guid? ProspectoId,
    Guid? SocioId,
    string? Observacoes);

public record EnvioDocumentoResultado(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Mensagem);

public record ValidacaoDocumentoResultado(
    bool Valido,
    decimal ConfiancaOcr,
    List<DivergenciaDocumento> Divergencias,
    bool FallbackManual,
    string? Mensagem);

public record DivergenciaDocumento(
    string Campo,
    string ValorDocumento,
    string ValorCadastro,
    decimal Similaridade);
