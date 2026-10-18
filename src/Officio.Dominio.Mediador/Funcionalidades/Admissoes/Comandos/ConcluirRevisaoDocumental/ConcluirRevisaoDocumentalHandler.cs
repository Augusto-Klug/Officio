using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.ConcluirRevisaoDocumental;

public sealed class ConcluirRevisaoDocumentalHandler(IAdmissaoServico servico)
    : IRequestHandler<ConcluirRevisaoDocumentalComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(ConcluirRevisaoDocumentalComando request, CancellationToken ct)
        => servico.ConcluirRevisaoDocumentalAsync(request.AdmissaoId, ct);
}
