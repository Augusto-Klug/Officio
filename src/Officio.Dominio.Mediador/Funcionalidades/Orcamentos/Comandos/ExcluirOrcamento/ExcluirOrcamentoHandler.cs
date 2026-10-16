using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ExcluirOrcamento;

public class ExcluirOrcamentoHandler(IOrcamentoServico servico)
    : IRequestHandler<ExcluirOrcamentoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(ExcluirOrcamentoComando request, CancellationToken ct)
        => servico.ExcluirAsync(request.OrcamentoId, ct);
}
