using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Cancelar;

public class CancelarFaturamentoHandler(IFaturamentoServico servico)
    : IRequestHandler<CancelarFaturamentoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(CancelarFaturamentoComando request, CancellationToken cancellationToken)
        => servico.CancelarAsync(request.Id, cancellationToken);
}
