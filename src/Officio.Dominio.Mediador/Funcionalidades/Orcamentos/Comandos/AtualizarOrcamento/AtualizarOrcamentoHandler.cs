using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;

public class AtualizarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<AtualizarOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AtualizarOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new AtualizarOrcamentoRequisicao(
            request.OrcamentoId,
            request.Titulo,
            request.DiaVencimento,
            request.Observacoes,
            request.Itens.Select(item => new ItemOrcamentoRequisicao(
                item.ItemCatalogoOrcamentoId,
                item.Descricao,
                item.Secao,
                item.Tipo,
                item.Quantidade,
                item.ValorUnitario,
                item.Ordem)).ToList());

        return await orcamentoServico.AtualizarAsync(requisicao, ct);
    }
}
