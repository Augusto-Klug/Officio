using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IContabilidadeServico
{
    Task<Resultado<DadosContabilidadeDto>> ObterAsync(CancellationToken ct = default);
    Task<Resultado<DadosContabilidadeDto>> AtualizarAsync(DadosContabilidadeDto dados, CancellationToken ct = default);
}
