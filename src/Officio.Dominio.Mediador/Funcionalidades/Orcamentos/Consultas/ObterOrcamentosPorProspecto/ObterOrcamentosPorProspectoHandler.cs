using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public class ObterOrcamentosPorProspectoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<ObterOrcamentosPorProspectoConsulta, Resultado<List<OrcamentoDto>>>
{
    public async Task<Resultado<List<OrcamentoDto>>> Handle(
        ObterOrcamentosPorProspectoConsulta request,
        CancellationToken ct)
    {
        var resultado = await orcamentoServico.ObterPorProspectoAsync(request.ProspectoId, ct);

        return resultado.ParaResultado(orcamentos => orcamentos.Select(orcamento => new OrcamentoDto(
            orcamento.Id,
            orcamento.ProspectoId,
            orcamento.Titulo,
            orcamento.Status,
            orcamento.ValorTotal,
            orcamento.DiaVencimento,
            orcamento.Observacoes,
            orcamento.MotivoRecusa,
            orcamento.Itens.Select(item => new ItemOrcamentoDto(
                item.Id,
                item.ItemCatalogoOrcamentoId,
                item.Descricao,
                item.Secao,
                item.Tipo,
                item.Quantidade,
                item.ValorUnitario,
                item.ValorTotal,
                item.Ordem)).ToList())).ToList());
    }
}
