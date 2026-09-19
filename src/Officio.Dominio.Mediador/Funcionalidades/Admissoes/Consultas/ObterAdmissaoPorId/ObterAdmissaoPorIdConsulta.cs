using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterAdmissaoPorId;

public record ObterAdmissaoPorIdConsulta(Guid AdmissaoId) : IRequest<Resultado<AdmissaoDetalheDto>>;
