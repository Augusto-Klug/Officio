using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;

public class CriarOrcamentoHandler(IOrcamentoServico orcamentoServico)
    : IRequestHandler<CriarOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(CriarOrcamentoComando request, CancellationToken ct)
    {
        var requisicao = new CriarOrcamentoRequisicao(
            request.ProspectoId,
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

        return await orcamentoServico.CriarAsync(requisicao, ct);
    }
}
