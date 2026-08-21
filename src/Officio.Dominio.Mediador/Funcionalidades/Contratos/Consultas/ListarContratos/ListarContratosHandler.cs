using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

public class ListarContratosHandler(IContratoServico contratoServico)
    : IRequestHandler<ListarContratosConsulta, Resultado<List<ContratoDto>>>
{
    public async Task<Resultado<List<ContratoDto>>> Handle(
        ListarContratosConsulta request,
        CancellationToken ct)
        => (await contratoServico.ListarAsync(ct)).ParaResultado(
            contratos => contratos.Select(Mapear).ToList());

    private static ContratoDto Mapear(Dominio.Contratos.ContratoResultado contrato)
        => new(
            contrato.Id,
            contrato.Numero,
            contrato.ProspectoId,
            contrato.ClienteId,
            contrato.OrcamentoId,
            contrato.RazaoSocial,
            contrato.Status,
            contrato.ValorTotal,
            contrato.DiaVencimento,
            contrato.DataInicio,
            contrato.DataFim,
            contrato.RenovacaoAutomatica,
            contrato.ClausulasExtras,
            contrato.PdfGerado);
}
