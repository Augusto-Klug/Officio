using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;

public class AceitarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AceitarOrcamentoComando, Resultado<bool>>
{
    public async Task<Resultado<bool>> Handle(AceitarOrcamentoComando request, CancellationToken ct)
        => await orcamentoServico.AceitarAsync(request.OrcamentoId, request.ConfirmarSubstituicao, ct);
}
