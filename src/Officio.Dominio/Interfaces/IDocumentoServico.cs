using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IDocumentoServico
{
    Task<Resultado<ResultadoEnvioDocumentoDto>> EnviarAsync(
        EnviarDocumentoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<ValidationResultDto>> ValidarAsync(
        Guid documentoId,
        CancellationToken ct = default);
}
