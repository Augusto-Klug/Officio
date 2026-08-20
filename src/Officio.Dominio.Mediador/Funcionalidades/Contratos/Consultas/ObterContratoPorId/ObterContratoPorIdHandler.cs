using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ListarContratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterContratoPorId;

public class ObterContratoPorIdHandler(IContratoServico contratoServico)
    : IRequestHandler<ObterContratoPorIdConsulta, Resultado<ContratoDto>>
{
    public async Task<Resultado<ContratoDto>> Handle(
        ObterContratoPorIdConsulta request,
        CancellationToken ct)
        => (await contratoServico.ObterPorIdAsync(request.ContratoId, ct)).ParaResultado(Mapear);

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
