using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Resumo;

public record ObterResumoFaturamentoConsulta(DateOnly Competencia)
    : IRequest<Resultado<ResumoFaturamentoCompetenciaDto>>;
