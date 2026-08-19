using MediatR;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;

public class CriarOrcamentoHandler(
    IOrcamentoRepositorio repositorio,
    IProspectoRepositorio prospectoRepositorio,
    IEmpresaAtual empresaAtual) : IRequestHandler<CriarOrcamentoComando, Resultado<Guid>>
{
    public async Task<Resultado<Guid>> Handle(CriarOrcamentoComando request, CancellationToken ct)
    {
        if (!DiaVencimentoValido(request.DiaVencimento))
            return Resultado<Guid>.CriarFalha("Dia de vencimento deve estar entre 1 e 31.");

        if (await prospectoRepositorio.ObterComSociosAsync(request.ProspectoId, ct) is null)
            return Resultado<Guid>.CriarFalha("Prospecto nao encontrado.");

        var orcamento = Orcamento.Criar(
            empresaAtual.TenantId,
            request.ProspectoId,
            request.Titulo,
            request.DiaVencimento,
            request.Observacoes);
        var itens = await CriarItensAsync(request.Itens, orcamento.Id, ct);

        if (!itens.Sucesso)
            return Resultado<Guid>.CriarFalha(itens.Erro!);

        orcamento.SubstituirItens(itens.Valor!);
        await repositorio.AdicionarAsync(orcamento, ct);
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

    internal static bool DiaVencimentoValido(int diaVencimento)
        => diaVencimento is >= 1 and <= 31;
}
