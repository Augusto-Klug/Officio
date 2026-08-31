using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;

public class AlterarStatusOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AlterarStatusOrcamentoComando, Resultado<bool>>
{
    public async Task<Resultado<bool>> Handle(AlterarStatusOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AlterarStatusOrcamentoRequisicao(request.OrcamentoId, request.Status);

        return await orcamentoServico.AlterarStatusAsync(requisicao, ct);
    }
}
