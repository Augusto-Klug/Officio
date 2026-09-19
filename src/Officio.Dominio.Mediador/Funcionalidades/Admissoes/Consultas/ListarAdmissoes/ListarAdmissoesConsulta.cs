using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarAdmissoes;

public record ListarAdmissoesConsulta(Guid? ClienteId, string? Status, string? Busca)
    : IRequest<Resultado<List<AdmissaoResumoDto>>>;
