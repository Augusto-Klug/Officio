using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarEmpresasComAdmissoes;

public record ListarEmpresasComAdmissoesConsulta(string? Busca)
    : IRequest<Resultado<List<EmpresaAdmissoesResumoDto>>>;
