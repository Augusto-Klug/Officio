using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IFaturamentoServico
{
    Task<Resultado<ResultadoGeracaoFaturamentoDto>> GerarCompetenciaAsync(GerarFaturamentoCompetenciaRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<ResultadoRecalculoFaturamentoDto>> RecalcularCompetenciaAsync(GerarFaturamentoCompetenciaRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<ResultadoFechamentoFaturamentoDto>> FecharCompetenciaAsync(GerarFaturamentoCompetenciaRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<List<FaturamentoMensalDto>>> ListarAsync(DateOnly? competencia, string? status, CancellationToken ct = default);
    Task<Resultado<ResumoFaturamentoCompetenciaDto>> ObterResumoAsync(DateOnly competencia, CancellationToken ct = default);
    Task<Resultado<bool>> FecharAsync(Guid id, CancellationToken ct = default);
    Task<Resultado<bool>> CancelarAsync(Guid id, CancellationToken ct = default);
}
