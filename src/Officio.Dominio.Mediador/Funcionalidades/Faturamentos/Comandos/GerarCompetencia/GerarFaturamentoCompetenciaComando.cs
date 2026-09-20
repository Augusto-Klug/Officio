using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.GerarCompetencia;

public record GerarFaturamentoCompetenciaComando(DateOnly Competencia)
    : IRequest<Resultado<ResultadoGeracaoFaturamentoDto>>;
