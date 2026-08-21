using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;

public class AceitarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AceitarOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(AceitarOrcamentoComando request, CancellationToken ct)
        => (await orcamentoServico.AceitarAsync(request.OrcamentoId, ct)).ParaUnit();
}
