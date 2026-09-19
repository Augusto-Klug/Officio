using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Consultas.ListarEmpresas;

public record ListarEmpresasConsulta : IRequest<Resultado<List<EmpresaPlataformaDto>>>;
