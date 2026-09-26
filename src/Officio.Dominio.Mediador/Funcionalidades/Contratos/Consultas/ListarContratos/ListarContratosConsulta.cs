using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public record ListarContratosConsulta(Guid? EmpresaId = null) : IRequest<Resultado<List<ContratoDto>>>;
