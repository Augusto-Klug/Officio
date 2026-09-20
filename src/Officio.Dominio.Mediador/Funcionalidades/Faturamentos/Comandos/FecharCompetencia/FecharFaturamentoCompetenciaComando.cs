using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.FecharCompetencia;

public record FecharFaturamentoCompetenciaComando(DateOnly Competencia)
    : IRequest<Resultado<ResultadoFechamentoFaturamentoDto>>;
