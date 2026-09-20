using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AprovarDocumentoAdmissao;

public class AprovarDocumentoAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<AprovarDocumentoAdmissaoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(
        AprovarDocumentoAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.AprovarManualmenteAsync(request.Requisicao, cancellationToken);
}
