using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;

public class RecusarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<RecusarOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(RecusarOrcamentoComando request, CancellationToken ct)
        => (await orcamentoServico.RecusarAsync(
            new RecusarOrcamentoRequisicao(request.OrcamentoId, request.Motivo),
            ct)).ParaUnit();
}
