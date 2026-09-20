using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.RecalcularCompetencia;

public class RecalcularFaturamentoCompetenciaHandler(IFaturamentoServico servico)
    : IRequestHandler<RecalcularFaturamentoCompetenciaComando, Resultado<ResultadoRecalculoFaturamentoDto>>
{
    public Task<Resultado<ResultadoRecalculoFaturamentoDto>> Handle(
        RecalcularFaturamentoCompetenciaComando request,
        CancellationToken ct)
        => servico.RecalcularCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(request.Competencia), ct);
}
