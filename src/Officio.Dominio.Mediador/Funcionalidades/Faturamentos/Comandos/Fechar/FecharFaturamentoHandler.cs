using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Comandos.Fechar;

public class FecharFaturamentoHandler(IFaturamentoServico servico)
    : IRequestHandler<FecharFaturamentoComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(FecharFaturamentoComando request, CancellationToken cancellationToken)
        => servico.FecharAsync(request.Id, cancellationToken);
}
