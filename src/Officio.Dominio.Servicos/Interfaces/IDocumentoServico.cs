using Officio.Dominio.Servicos.Comum;
using Officio.Dominio.Servicos.Modelos;

namespace Officio.Dominio.Servicos.Interfaces;

public interface IDocumentoServico
{
    Task<ResultadoOperacao<EnvioDocumentoResultado>> EnviarAsync(
        EnviarDocumentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<ValidacaoDocumentoResultado>> ValidarAsync(
        Guid documentoId,
        CancellationToken ct = default);
}
