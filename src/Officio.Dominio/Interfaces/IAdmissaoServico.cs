using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IAdmissaoServico
{
    Task<Resultado<Guid>> CriarManualAsync(CriarAdmissaoManualRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<Guid>> AtualizarAsync(AtualizarAdmissaoRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<List<AdmissaoResumoDto>>> ListarAsync(Guid? clienteId, string? status, string? busca, CancellationToken ct = default);
    Task<Resultado<AdmissaoDetalheDto>> ObterPorIdAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<bool>> ConcluirAsync(Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<bool>> CancelarAsync(Guid admissaoId, CancellationToken ct = default);
}
