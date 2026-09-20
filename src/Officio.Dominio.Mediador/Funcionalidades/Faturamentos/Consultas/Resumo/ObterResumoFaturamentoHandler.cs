using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Resumo;

public class ObterResumoFaturamentoHandler(IFaturamentoServico servico)
    : IRequestHandler<ObterResumoFaturamentoConsulta, Resultado<ResumoFaturamentoCompetenciaDto>>
{
    public Task<Resultado<ResumoFaturamentoCompetenciaDto>> Handle(
        ObterResumoFaturamentoConsulta request,
        CancellationToken ct)
        => servico.ObterResumoAsync(request.Competencia, ct);
}
