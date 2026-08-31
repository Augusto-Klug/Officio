using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public record ListarContratosConsulta : IRequest<Resultado<List<ContratoDto>>>;
