using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarEmpresasContratos;

public sealed record ListarEmpresasContratosConsulta
    : IRequest<Resultado<List<EmpresaContratoFiltroDto>>>;
