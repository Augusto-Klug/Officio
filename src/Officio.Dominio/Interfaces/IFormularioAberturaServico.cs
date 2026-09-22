using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IFormularioAberturaServico
{
    Task<Resultado<FormularioAberturaDto>> ObterPorProspectoAsync(Guid prospectoId, CancellationToken ct = default);
    Task<Resultado<FormularioAberturaDto>> AtualizarAsync(AtualizarFormularioAberturaRequisicao requisicao, CancellationToken ct = default);
}
