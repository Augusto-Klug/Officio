namespace Officio.Dominio.Contratos.Dtos;

public record AdmissaoDocumentoDto(
    Guid Id,
    Guid TipoDocumentoId,
    string TipoCodigo,
    string TipoDescricao,
    string Rotulo,
    bool Obrigatorio,
    int Ordem,
    Guid? DocumentoId,
    string? NomeArquivo,
    string? MimeType,
    long? TamanhoBytes,
    decimal? QualidadeEstimada,
    DateTime? EnviadoEm,
    string StatusValidacao,
    decimal? ConfiancaOcr,
    string? MensagemValidacao);

public record ResumoValidacaoDocumentosAdmissaoDto(
    int Processados,
    int Aprovados,
    int PendentesRevisao,
    int Falhas);
