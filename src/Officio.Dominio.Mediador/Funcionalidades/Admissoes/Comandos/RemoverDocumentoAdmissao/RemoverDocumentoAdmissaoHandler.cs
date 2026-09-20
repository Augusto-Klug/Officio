using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.RemoverDocumentoAdmissao;

public class RemoverDocumentoAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<RemoverDocumentoAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        RemoverDocumentoAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.RemoverAsync(request.AdmissaoId, request.AdmissaoDocumentoId, cancellationToken);
}
