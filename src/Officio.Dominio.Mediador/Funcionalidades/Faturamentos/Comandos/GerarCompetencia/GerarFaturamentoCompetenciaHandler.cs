using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.GerarCompetencia;

public class GerarFaturamentoCompetenciaHandler(IFaturamentoServico servico)
    : IRequestHandler<GerarFaturamentoCompetenciaComando, Resultado<ResultadoGeracaoFaturamentoDto>>
{
    public Task<Resultado<ResultadoGeracaoFaturamentoDto>> Handle(
        GerarFaturamentoCompetenciaComando request,
        CancellationToken cancellationToken)
        => servico.GerarCompetenciaAsync(new GerarFaturamentoCompetenciaRequisicao(request.Competencia), cancellationToken);
}
