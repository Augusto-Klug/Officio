using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IFichaAdmissaoServico
{
    Task<Resultado<GerarLinkFichaAdmissaoDto>> GerarLinkAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<bool>> RevogarLinkAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<FichaAdmissaoPublicaDto>> ObterAsync(string token, CancellationToken ct = default);
    Task<Resultado<bool>> AtualizarAsync(string token, AtualizarFichaAdmissaoRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> ConcluirAsync(string token, CancellationToken ct = default);
    Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarDocumentoAsync(string token, Guid itemId, Stream arquivo, string nomeArquivo, string mimeType, long tamanhoBytes, CancellationToken ct = default);
    Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlDocumentoAsync(string token, Guid itemId, CancellationToken ct = default);
}
