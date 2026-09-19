using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentoPorId;

public class ObterOrcamentoPorIdHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ObterOrcamentoPorIdConsulta, Resultado<OrcamentoDto>>
{
    public async Task<Resultado<OrcamentoDto>> Handle(
        ObterOrcamentoPorIdConsulta request,
        CancellationToken ct)
        => await orcamentoServico.ObterPorIdAsync(request.OrcamentoId, ct);
}
