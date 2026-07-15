using Officio.Dominio.Servicos.Comum;
using Officio.Dominio.Servicos.Modelos;

namespace Officio.Dominio.Servicos.Interfaces;

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
