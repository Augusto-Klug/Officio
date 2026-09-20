using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Contratos;

public record EnviarDocumentoAdmissaoRequisicao(
    Guid AdmissaoId,
    Guid AdmissaoDocumentoId,
    Stream Arquivo,
    string NomeArquivo,
    string MimeType,
    long TamanhoBytes);

public record AprovarDocumentoAdmissaoRequisicao(
    Guid AdmissaoId,
    Guid AdmissaoDocumentoId,
    string? Observacao);

public record ResultadoUrlDocumentoAdmissaoDto(string Url, DateTime ExpiraEm);

public record ResultadoDocumentoAdmissaoDto(
    AdmissaoDocumentoDto Documento,
    string Mensagem);
