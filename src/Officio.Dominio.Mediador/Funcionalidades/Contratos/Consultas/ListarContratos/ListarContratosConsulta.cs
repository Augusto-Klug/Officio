using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Enumeradores;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public record ListarContratosConsulta(
    Guid? EmpresaId = null,
    StatusContrato? Status = null) : IRequest<Resultado<List<ContratoDto>>>;
