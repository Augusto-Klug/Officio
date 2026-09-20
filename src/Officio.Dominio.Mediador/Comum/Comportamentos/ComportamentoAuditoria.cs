using MediatR;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Comum.Comportamentos;

public class ComportamentoAuditoria<TRequest, TResponse>(
    IContextoAuditoria contextoAuditoria,
    IUsuarioAtual usuarioAtual)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!EhComando())
            return await next(cancellationToken);

        Guid? usuarioId = usuarioAtual.UserId == Guid.Empty ? null : usuarioAtual.UserId;
        using var _ = contextoAuditoria.Iniciar(typeof(TRequest).Name, usuarioId);

        return await next(cancellationToken);
    }

    private static bool EhComando()
        => typeof(TRequest).Name.EndsWith("Comando", StringComparison.Ordinal);
}
