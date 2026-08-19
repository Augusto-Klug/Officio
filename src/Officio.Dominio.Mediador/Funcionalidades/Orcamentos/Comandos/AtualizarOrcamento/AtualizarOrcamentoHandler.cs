using MediatR;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;

public class AtualizarOrcamentoHandler(
    IOrcamentoRepositorio repositorio,
    IEmpresaAtual empresaAtual) : IRequestHandler<AtualizarOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(AtualizarOrcamentoComando request, CancellationToken ct)
    {
        if (!CriarOrcamentoHandler.DiaVencimentoValido(request.DiaVencimento))
            return Resultado<Guid>.CriarFalha("Dia de vencimento deve estar entre 1 e 31.");

        var orcamento = await repositorio.ObterPorIdComItensAsync(request.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Guid>.CriarFalha("Orcamento nao encontrado.");

        if (orcamento.Status is not (StatusOrcamento.Rascunho or StatusOrcamento.EmRevisao))
            return Resultado<Guid>.CriarFalha("Orcamento nao pode ser editado no status atual.");

        var itens = await CriarItensAsync(request.Itens, orcamento.Id, ct);
        if (!itens.Sucesso)
            return Resultado<Guid>.CriarFalha(itens.Erro!);

        orcamento.Atualizar(request.Titulo, request.DiaVencimento, request.Observacoes);
        orcamento.SubstituirItens(itens.Valor!);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(orcamento.Id);
    }

    private async Task<Resultado<List<ItemOrcamento>>> CriarItensAsync(
        IEnumerable<CriarItemOrcamentoDto> itens,
        Guid orcamentoId,
        CancellationToken ct)
    {
        var snapshots = new List<ItemOrcamento>();

        foreach (var item in itens)
        {
            if (item.Quantidade < 0 || item.ValorUnitario < 0)
                return Resultado<List<ItemOrcamento>>.CriarFalha("Quantidade e valor unitario nao podem ser negativos.");

            if (item.ItemCatalogoOrcamentoId is Guid itemCatalogoId)
            {
                var itemCatalogo = await repositorio.ObterItemCatalogoAsync(itemCatalogoId, ct);
                if (itemCatalogo is null || !itemCatalogo.Ativo)
                    return Resultado<List<ItemOrcamento>>.CriarFalha("Item de catalogo ativo nao encontrado.");

                snapshots.Add(ItemOrcamento.CriarSnapshot(
                    empresaAtual.TenantId,
                    orcamentoId,
                    itemCatalogo.Id,
                    itemCatalogo.Descricao,
                    itemCatalogo.Secao,
                    itemCatalogo.Tipo,
                    item.Quantidade,
                    itemCatalogo.ValorReferencia ?? 0m,
                    item.Ordem));
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Descricao) || item.Secao is null || item.Tipo is null)
                return Resultado<List<ItemOrcamento>>.CriarFalha("Descricao, secao e tipo sao obrigatorios para item manual.");

            snapshots.Add(ItemOrcamento.CriarSnapshot(
                empresaAtual.TenantId,
                orcamentoId,
                null,
                item.Descricao,
                item.Secao.Value,
                item.Tipo.Value,
                item.Quantidade,
                item.ValorUnitario,
                item.Ordem));
        }

        return Resultado<List<ItemOrcamento>>.CriarSucesso(snapshots);
    }
}
