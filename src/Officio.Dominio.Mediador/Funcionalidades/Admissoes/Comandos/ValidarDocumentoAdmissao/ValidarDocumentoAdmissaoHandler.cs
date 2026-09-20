using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ValidarDocumentoAdmissao;

public class ValidarDocumentoAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<ValidarDocumentoAdmissaoComando, Resultado<ValidationResultDto>>
{
    public Task<Resultado<ValidationResultDto>> Handle(
        ValidarDocumentoAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.ValidarAsync(request.AdmissaoId, request.AdmissaoDocumentoId, cancellationToken);
}
