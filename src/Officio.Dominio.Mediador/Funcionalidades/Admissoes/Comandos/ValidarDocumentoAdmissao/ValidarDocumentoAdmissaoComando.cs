using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ValidarDocumentoAdmissao;

public record ValidarDocumentoAdmissaoComando(Guid AdmissaoId, Guid AdmissaoDocumentoId)
    : IRequest<Resultado<ValidationResultDto>>;
