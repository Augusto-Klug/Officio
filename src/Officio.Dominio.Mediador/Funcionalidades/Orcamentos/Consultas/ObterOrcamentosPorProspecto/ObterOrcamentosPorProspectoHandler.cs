using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Consultas.ObterOrcamentosPorProspecto;

public class ObterOrcamentosPorProspectoHandler(IOrcamentoRepositorio repositorio)
    : IRequestHandler<ObterOrcamentosPorProspectoConsulta, Resultado<List<OrcamentoDto>>>
{
    public async Task<Resultado<List<OrcamentoDto>>> Handle(
        ObterOrcamentosPorProspectoConsulta request,
        CancellationToken ct)
    {
        var orcamentos = await repositorio.ObterPorProspectoAsync(request.ProspectoId, ct);
        var dtos = orcamentos.Select(orcamento => new OrcamentoDto(
            orcamento.Id,
            orcamento.ProspectId,
            orcamento.Titulo,
            orcamento.Status.ToString(),
            orcamento.ValorTotal,
            orcamento.DiaVencimento,
            orcamento.Observacoes,
            orcamento.MotivoRecusa,
            orcamento.Itens.Select(item => new ItemOrcamentoDto(
                item.Id,
                item.ItemCatalogoOrcamentoId,
                item.Descricao,
                item.Secao.ToString(),
                item.Tipo.ToString(),
                item.Quantidade,
                item.ValorUnitario,
                item.ValorTotal,
                item.Ordem)).ToList())).ToList();

        return Resultado<List<OrcamentoDto>>.CriarSucesso(dtos);
    }
}
