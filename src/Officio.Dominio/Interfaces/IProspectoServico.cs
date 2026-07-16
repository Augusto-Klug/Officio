using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Interfaces;

public interface IProspectoServico
{
    Task<ResultadoOperacao<ProspectoCriadoResultado>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default);

    Task<ResultadoOperacao<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default);

    Task<ResultadoOperacao<ResultadoPaginado<ProspectoResumo>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default);
}
