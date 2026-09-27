using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IAdmissaoDocumentoServico
{
    Task<Resultado<List<AdmissaoDocumentoDto>>> ListarAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarAsync(EnviarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default);
    Task<Resultado<ValidationResultDto>> ValidarAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default);
    Task<Resultado<ResumoValidacaoDocumentosAdmissaoDto>> ValidarPendentesAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<bool>> AprovarManualmenteAsync(AprovarDocumentoAdmissaoRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> RemoverAsync(Guid admissaoId, Guid admissaoDocumentoId, CancellationToken ct = default);
}

public interface IConversorImagensPdf
{
    byte[] CriarPdf(IReadOnlyList<byte[]> imagens);
}
