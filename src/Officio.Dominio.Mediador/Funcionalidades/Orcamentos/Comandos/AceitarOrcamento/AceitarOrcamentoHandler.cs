using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AceitarOrcamento;

public class AceitarOrcamentoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<AceitarOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(AceitarOrcamentoComando request, CancellationToken ct)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(request.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Unit>.CriarFalha("Orcamento nao encontrado.");

        try
        {
            orcamento.Aceitar();
        }
        catch (InvalidOperationException exception)
        {
            return Resultado<Unit>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<Unit>.CriarSucesso(Unit.Value);
    }
}
