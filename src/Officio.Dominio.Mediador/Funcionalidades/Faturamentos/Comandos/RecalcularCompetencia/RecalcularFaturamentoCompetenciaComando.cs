using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.RecalcularCompetencia;

public record RecalcularFaturamentoCompetenciaComando(DateOnly Competencia)
    : IRequest<Resultado<ResultadoRecalculoFaturamentoDto>>;
