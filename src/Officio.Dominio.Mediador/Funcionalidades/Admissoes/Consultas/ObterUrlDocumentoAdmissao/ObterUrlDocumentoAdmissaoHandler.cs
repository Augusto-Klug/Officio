using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ObterUrlDocumentoAdmissao;

public class ObterUrlDocumentoAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<ObterUrlDocumentoAdmissaoConsulta, Resultado<ResultadoUrlDocumentoAdmissaoDto>>
{
    public Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> Handle(
        ObterUrlDocumentoAdmissaoConsulta request,
        CancellationToken cancellationToken)
        => servico.ObterUrlAsync(request.AdmissaoId, request.AdmissaoDocumentoId, cancellationToken);
}
