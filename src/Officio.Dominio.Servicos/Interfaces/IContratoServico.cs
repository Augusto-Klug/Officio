using Officio.Dominio.Servicos.Comum;
using Officio.Dominio.Servicos.Modelos;

namespace Officio.Dominio.Servicos.Interfaces;

public interface IContratoServico
{
    Task<ResultadoOperacao<PdfContratoResultado>> GerarPdfAsync(
        Guid contratoId,
        CancellationToken ct = default);

    Task<ResultadoOperacao<EnvioAssinaturaResultado>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken ct = default);
}
