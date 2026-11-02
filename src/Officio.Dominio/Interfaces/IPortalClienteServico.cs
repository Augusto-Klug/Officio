using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IPortalClienteServico
{
    Task<Resultado<VisaoPortalClienteDto>> ObterVisaoAsync(Guid? empresaId, CancellationToken ct = default);
    Task<Resultado<AdmissaoDetalheDto>> ObterAdmissaoAsync(Guid empresaId, Guid admissaoId, CancellationToken ct = default);
    Task<Resultado<ResultadoPdfAssinadoContratoDto>> ObterPdfAssinadoAsync(Guid empresaId, Guid contratoId, CancellationToken ct = default);
    Task<Resultado<AdmissaoPortalCriadaDto>> CriarAdmissaoAsync(CriarAdmissaoPortalRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> AtualizarDadosEmpregadorAsync(Guid admissaoId, AtualizarFichaEmpresaRequisicao requisicao, CancellationToken ct = default);
    Task<Resultado<bool>> ConcluirDadosEmpregadorAsync(Guid admissaoId, CancellationToken ct = default);
}
