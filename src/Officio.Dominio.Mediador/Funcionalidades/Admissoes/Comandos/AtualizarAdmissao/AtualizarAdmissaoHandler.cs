using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Admissoes.Comandos.AtualizarAdmissao;

public class AtualizarAdmissaoHandler(IAdmissaoServico servico)
    : IRequestHandler<AtualizarAdmissaoComando, Resultado<Guid>>
{
    public Task<Resultado<Guid>> Handle(AtualizarAdmissaoComando request, CancellationToken ct)
        => servico.AtualizarAsync(new AtualizarAdmissaoRequisicao(request.AdmissaoId, request.Dados), ct);
}
