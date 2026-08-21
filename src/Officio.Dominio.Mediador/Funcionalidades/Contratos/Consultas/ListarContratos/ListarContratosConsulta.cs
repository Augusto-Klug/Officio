using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public record ListarContratosConsulta : IRequest<Resultado<List<ContratoDto>>>;
