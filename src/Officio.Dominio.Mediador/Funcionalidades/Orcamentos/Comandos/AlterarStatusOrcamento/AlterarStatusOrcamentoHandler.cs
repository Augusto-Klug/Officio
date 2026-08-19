using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;

public class AlterarStatusOrcamentoHandler(
    IOrcamentoRepositorio repositorio,
    IProspectoRepositorio prospectoRepositorio)
    : IRequestHandler<AlterarStatusOrcamentoComando, Resultado<Unit>>
{
    public async Task<Resultado<Unit>> Handle(AlterarStatusOrcamentoComando request, CancellationToken ct)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(request.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Unit>.CriarFalha("Orcamento nao encontrado.");

        if (request.Status == StatusOrcamento.Enviado)
        {
            var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
            if (prospecto is null)
                return Resultado<Unit>.CriarFalha("Prospecto nao encontrado.");

            prospecto.UpdateStatus(StatusProspecto.PropostaEnviada);
        }

        orcamento.AlterarStatus(request.Status);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Unit>.CriarSucesso(Unit.Value);
    }
}
