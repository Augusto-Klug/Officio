using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.ConverterParaContrato;

public class ConverterOrcamentoParaContratoHandler(IContratoServico contratoServico)
    : IRequestHandler<ConverterOrcamentoParaContratoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(
        ConverterOrcamentoParaContratoComando request,
        CancellationToken ct)
    {
        var requisicao = new ConverterOrcamentoContratoRequisicao(
            request.OrcamentoId,
            request.DataInicio,
            request.DataFim,
            request.RenovacaoAutomatica,
            request.ClausulasExtras);

        return (await contratoServico.ConverterOrcamentoAsync(requisicao, ct)).ParaResultado();
    }
}
