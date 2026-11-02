using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IFichaAdmissaoServico
{
    Task<Resultado<GerarLinkFichaAdmissaoDto>> GerarLinkAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<bool>> EnviarLinkPorEmailAsync(EnviarLinkFichaAdmissaoEmailRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> RevogarLinkAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<FichaFuncionarioPublicaDto>> ObterAsync(string token, CancellationToken ct = default);
    Task<Resultado<bool>> AtualizarAsync(string token, AtualizarFichaFuncionarioRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> ConcluirAsync(string token, CancellationToken ct = default);
    Task<Resultado<bool>> AtualizarEmpresaPortalAsync(Guid admissaoId, AtualizarFichaEmpresaRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> ConcluirEmpresaPortalAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarDocumentoAsync(string token, Guid itemId, IReadOnlyList<ArquivoAdmissaoUpload> arquivos, CancellationToken ct = default);
    Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlDocumentoAsync(string token, Guid itemId, CancellationToken ct = default);
}
