using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IProspectoServico
{
    Task<Resultado<bool>> AtualizarSociosAsync(Guid id, CriarProspectoRequisicao requisicao, CancellationToken ct = default);
    Task<List<TipoRelacaoSocietariaDto>> ListarTiposRelacaoSocietariaAsync(CancellationToken ct = default);
    Task<Resultado<ProspectoDetalheDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default, bool incluirSociosCliente = false);
    Task<Resultado<bool>> AtualizarAsync(Guid id, CriarProspectoRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> ExcluirAsync(Guid id, CancellationToken ct = default);

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
