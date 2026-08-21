using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;

public class AlterarStatusOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AlterarStatusOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(AlterarStatusOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AlterarStatusOrcamentoRequisicao(request.OrcamentoId, request.Status);

        return (await orcamentoServico.AlterarStatusAsync(requisicao, ct)).ParaUnit();
    }
}
