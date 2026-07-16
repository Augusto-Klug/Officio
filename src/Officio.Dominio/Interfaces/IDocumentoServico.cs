using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Interfaces;

public interface IDocumentoServico
{
    Task<ResultadoOperacao<EnvioDocumentoResultado>> EnviarAsync(
        EnviarDocumentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<ValidacaoDocumentoResultado>> ValidarAsync(
        Guid documentoId,
        CancellationToken ct = default);
}
