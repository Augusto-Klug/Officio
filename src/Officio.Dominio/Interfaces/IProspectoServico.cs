using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IProspectoServico
{
    Task<Resultado<ProspectoCriadoDto>> CriarAsync(
        CriarProspectoRequisicao requisicao,
        CancellationToken ct = default);

    Task<Resultado<Guid>> ConverterParaClienteAsync(
        Guid prospectoId,
        CancellationToken ct = default);

    Task<Resultado<PagedResult<ProspectoDto>>> ObterAsync(
        ObterProspectosFiltro filtro,
        CancellationToken ct = default);
}
