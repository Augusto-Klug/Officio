using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;

public class RecusarOrcamentoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<RecusarOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(RecusarOrcamentoComando request, CancellationToken ct)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(request.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Unit>.CriarFalha("Orcamento nao encontrado.");

        try
        {
            orcamento.Recusar(request.Motivo);
        }
        catch (ArgumentException exception)
        {
            return Resultado<Unit>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);
        return Resultado<Unit>.CriarSucesso(Unit.Value);
    }
}
