using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;

public class RecusarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<RecusarOrcamentoComando, Resultado<bool>>
{
    public async Task<Resultado<bool>> Handle(RecusarOrcamentoComando request, CancellationToken ct)
        => await orcamentoServico.RecusarAsync(
            new RecusarOrcamentoRequisicao(request.OrcamentoId, request.Motivo),
            ct);
}
