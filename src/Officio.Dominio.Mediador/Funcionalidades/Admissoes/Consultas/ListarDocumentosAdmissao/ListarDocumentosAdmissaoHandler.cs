using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Consultas.ListarDocumentosAdmissao;

public class ListarDocumentosAdmissaoHandler(IAdmissaoDocumentoServico servico)
    : IRequestHandler<ListarDocumentosAdmissaoConsulta, Resultado<List<AdmissaoDocumentoDto>>>
{
    public Task<Resultado<List<AdmissaoDocumentoDto>>> Handle(
        ListarDocumentosAdmissaoConsulta request,
        CancellationToken cancellationToken)
        => servico.ListarAsync(request.AdmissaoId, cancellationToken);
}
