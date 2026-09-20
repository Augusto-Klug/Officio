using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.FecharCompetencia;

public class FecharFaturamentoCompetenciaHandler(IFaturamentoServico servico)
    : IRequestHandler<FecharFaturamentoCompetenciaComando, Resultado<ResultadoFechamentoFaturamentoDto>>
{
    public Task<Resultado<ResultadoFechamentoFaturamentoDto>> Handle(
        FecharFaturamentoCompetenciaComando request,
        CancellationToken ct)
        => servico.FecharCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(request.Competencia), ct);
}
