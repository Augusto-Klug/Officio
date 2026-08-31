using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public class ObterOrcamentosPorProspectoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ObterOrcamentosPorProspectoConsulta, Resultado<List<OrcamentoDto>>>
{
    public async Task<Resultado<List<OrcamentoDto>>> Handle(
        ObterOrcamentosPorProspectoConsulta request,
        CancellationToken ct)
        => await orcamentoServico.ObterPorProspectoAsync(request.ProspectoId, ct);
}
