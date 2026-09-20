using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.EnviarDocumentoAdmissao;

public class EnviarDocumentoAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<EnviarDocumentoAdmissaoComando, Resultado<ResultadoDocumentoAdmissaoDto>>
{
    public Task<Resultado<ResultadoDocumentoAdmissaoDto>> Handle(
        EnviarDocumentoAdmissaoComando request,
        CancellationToken cancellationToken)
        => servico.EnviarAsync(request.Requisicao, cancellationToken);
}
