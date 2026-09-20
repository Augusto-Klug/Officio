using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterUrlDocumentoAdmissao;

public record ObterUrlDocumentoAdmissaoConsulta(Guid AdmissaoId, Guid AdmissaoDocumentoId)
    : IRequest<Resultado<ResultadoUrlDocumentoAdmissaoDto>>;
