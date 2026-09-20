using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Listar;

public record ListarFaturamentosConsulta(DateOnly? Competencia, string? Status)
    : IRequest<Resultado<List<FaturamentoMensalDto>>>;
