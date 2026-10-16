using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public class OrcamentoServico(
    IOrcamentoRepositorio repositorio,
    IProspectoRepositorio prospectoRepositorio,
    IEmpresaAtual empresaAtual) : IOrcamentoServico
{
    public async Task<Resultado<Guid>> CriarAsync(CriarOrcamentoRequisicao requisicao, CancellationToken ct = default)
    {
        if (!DiaVencimentoValido(requisicao.DiaVencimento))
            return Resultado<Guid>.CriarFalha("Dia de vencimento deve estar entre 1 e 31.");

        if (await prospectoRepositorio.ObterComSociosAsync(requisicao.ProspectoId, ct) is null)
            return Resultado<Guid>.CriarFalha("Prospecto nao encontrado.");

        var orcamento = Orcamento.Criar(
            empresaAtual.TenantId,
            requisicao.ProspectoId,
            requisicao.Titulo,
            requisicao.DiaVencimento,
            requisicao.Observacoes);
        var itens = await CriarItensParaNovoOrcamentoAsync(requisicao.Itens, orcamento.Id, ct);

        if (!itens.Sucesso)
            return Resultado<Guid>.CriarFalha(itens.Erro!);

        orcamento.SubstituirItens(itens.Valor!);
        await repositorio.AdicionarAsync(orcamento, ct);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(orcamento.Id);
    }

    public async Task<Resultado<Guid>> AtualizarAsync(
        AtualizarOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (!DiaVencimentoValido(requisicao.DiaVencimento))
            return Resultado<Guid>.CriarFalha("Dia de vencimento deve estar entre 1 e 31.");

        var orcamento = await repositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Guid>.CriarFalha("Orcamento nao encontrado.");

        if (orcamento.Status is not (StatusOrcamento.Rascunho or StatusOrcamento.EmRevisao))
            return Resultado<Guid>.CriarFalha("Orcamento nao pode ser editado no status atual.");

        var itens = await CriarItensParaAtualizacaoAsync(requisicao.Itens, orcamento, ct);
        if (!itens.Sucesso)
            return Resultado<Guid>.CriarFalha(itens.Erro!);

        orcamento.Atualizar(requisicao.Titulo, requisicao.DiaVencimento, requisicao.Observacoes);
        orcamento.SubstituirItens(itens.Valor!);
        await repositorio.AdicionarItensAsync(itens.Valor!, ct);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(orcamento.Id);
    }

    public async Task<Resultado<List<OrcamentoDto>>> ObterPorProspectoAsync(
        Guid prospectoId,
        CancellationToken ct = default)
    {
        if (await prospectoRepositorio.ObterComSociosAsync(prospectoId, ct) is null)
            return Resultado<List<OrcamentoDto>>.CriarFalha("Prospecto nao encontrado.");

        var orcamentos = await repositorio.ObterPorProspectoAsync(prospectoId, ct);
        var resultados = orcamentos.Select(MapearOrcamento).ToList();

        return Resultado<List<OrcamentoDto>>.CriarSucesso(resultados);
    }

    public async Task<Resultado<OrcamentoDto>> ObterPorIdAsync(
        Guid orcamentoId,
        CancellationToken ct = default)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(orcamentoId, ct);

        return orcamento is null
            ? Resultado<OrcamentoDto>.CriarFalha("Orcamento nao encontrado.")
            : Resultado<OrcamentoDto>.CriarSucesso(MapearOrcamento(orcamento));
    }

    public async Task<Resultado<Guid>> CriarItemCatalogoAsync(
        CriarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (requisicao.Tipo == TipoItemOrcamento.ItemMonetario && !requisicao.ValorReferencia.HasValue)
            return Resultado<Guid>.CriarFalha("Item monetario deve possuir valor de referencia.");

        if (!NaturezaCobrancaValida(requisicao.Tipo, requisicao.NaturezaCobranca))
            return Resultado<Guid>.CriarFalha("Somente itens monetarios podem compor o faturamento recorrente.");

        var item = ItemCatalogoOrcamento.Criar(
            empresaAtual.TenantId,
            requisicao.Descricao,
            requisicao.Secao,
            requisicao.Tipo,
            requisicao.ValorReferencia,
            requisicao.NaturezaCobranca);

        await repositorio.AdicionarItemCatalogoAsync(item, ct);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }

    public async Task<Resultado<Guid>> AtualizarItemCatalogoAsync(
        AtualizarItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterItemCatalogoAsync(requisicao.ItemId, ct);
        if (item is null)
            return Resultado<Guid>.CriarFalha("Item de catalogo nao encontrado.");

        if (requisicao.Tipo == TipoItemOrcamento.ItemMonetario && !requisicao.ValorReferencia.HasValue)
            return Resultado<Guid>.CriarFalha("Item monetario deve possuir valor de referencia.");

        if (!NaturezaCobrancaValida(requisicao.Tipo, requisicao.NaturezaCobranca))
            return Resultado<Guid>.CriarFalha("Somente itens monetarios podem compor o faturamento recorrente.");

        item.Atualizar(
            requisicao.Descricao,
            requisicao.Secao,
            requisicao.Tipo,
            requisicao.ValorReferencia,
            requisicao.NaturezaCobranca);
        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }

    public async Task<Resultado<Guid>> AlterarAtivoItemCatalogoAsync(
        AlterarAtivoItemCatalogoOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterItemCatalogoAsync(requisicao.ItemId, ct);
        if (item is null)
            return Resultado<Guid>.CriarFalha("Item de catalogo nao encontrado.");

        if (requisicao.Ativo)
            item.Ativar();
        else
            item.Desativar();

        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(item.Id);
    }

    public async Task<Resultado<List<CatalogoOrcamentoDto>>> ListarCatalogoAsync(
        bool incluirInativos = false,
        CancellationToken ct = default)
    {
        var itens = await repositorio.ObterCatalogoAsync(incluirInativos, ct);
        var catalogo = itens
            .Select(item => new CatalogoOrcamentoDto(
                item.Id,
                item.Descricao,
                item.Secao.ToString(),
                item.Tipo.ToString(),
                item.NaturezaCobranca.ToString(),
                item.ValorReferencia,
                item.Ativo))
            .ToList();

        return Resultado<List<CatalogoOrcamentoDto>>.CriarSucesso(catalogo);
    }

    public async Task<Resultado<bool>> AlterarStatusAsync(
        AlterarStatusOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<bool>.CriarFalha("Orcamento nao encontrado.");

        try
        {
            orcamento.AlterarStatus(requisicao.Status);
        }
        catch (InvalidOperationException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        if (requisicao.Status == StatusOrcamento.Enviado)
        {
            var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
            if (prospecto is null)
                return Resultado<bool>.CriarFalha("Prospecto nao encontrado.");

            prospecto.UpdateStatus(StatusProspecto.PropostaEnviada);
        }

        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> AceitarAsync(Guid orcamentoId, CancellationToken ct = default)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(orcamentoId, ct);
        if (orcamento is null)
            return Resultado<bool>.CriarFalha("Orcamento nao encontrado.");

        try
        {
            orcamento.Aceitar();
        }
        catch (InvalidOperationException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> RecusarAsync(
        RecusarOrcamentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var orcamento = await repositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<bool>.CriarFalha("Orcamento nao encontrado.");

        try
        {
            orcamento.Recusar(requisicao.Motivo);
        }
        catch (ArgumentException exception)
        {
            return Resultado<bool>.CriarFalha(exception.Message);
        }

        await repositorio.SalvarAlteracoesAsync(ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task<ResultadoOperacao<List<ItemOrcamento>>> CriarItensParaNovoOrcamentoAsync(
        IEnumerable<ItemOrcamentoRequisicao> itens,
        Guid orcamentoId,
        CancellationToken ct)
    {
        var snapshots = new List<ItemOrcamento>();
        var catalogoIds = new HashSet<Guid>();

        foreach (var item in itens)
        {
            if (item.Quantidade < 1)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Quantidade deve ser maior que zero.");
            if (item.ValorUnitario < 0)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Valor unitario nao pode ser negativo.");

            if (item.ItemOrcamentoId is not null)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Itens existentes so podem ser informados ao atualizar um orcamento.");

            if (item.ItemCatalogoOrcamentoId is Guid itemCatalogoId)
            {
                if (!catalogoIds.Add(itemCatalogoId))
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Selecione cada item do catalogo apenas uma vez e ajuste a quantidade.");

                var itemCatalogo = await repositorio.ObterItemCatalogoAsync(itemCatalogoId, ct);
                if (itemCatalogo is null || !itemCatalogo.Ativo)
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Item de catalogo ativo nao encontrado.");
                if (itemCatalogo.Tipo == TipoItemOrcamento.ItemMonetario && !itemCatalogo.ValorReferencia.HasValue)
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Configure o valor de referencia do item de catalogo antes de inclui-lo no orcamento.");

                snapshots.Add(ItemOrcamento.CriarSnapshot(
                    empresaAtual.TenantId,
                    orcamentoId,
                    itemCatalogo.Id,
                    itemCatalogo.Descricao,
                    itemCatalogo.Secao,
                    itemCatalogo.Tipo,
                    item.Quantidade,
                    itemCatalogo.Tipo == TipoItemOrcamento.ItemMonetario ? item.ValorUnitario : 0m,
                    item.Ordem,
                    itemCatalogo.NaturezaCobranca));
                continue;
            }

            return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Selecione um item ativo do catalogo para cada linha do orcamento.");
        }

        if (snapshots.Count == 0)
            return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("O orcamento deve conter ao menos um item do catalogo.");

        return ResultadoOperacao<List<ItemOrcamento>>.CriarSucesso(snapshots);
    }

    private async Task<ResultadoOperacao<List<ItemOrcamento>>> CriarItensParaAtualizacaoAsync(
        IEnumerable<ItemOrcamentoRequisicao> itens,
        Orcamento orcamento,
        CancellationToken ct)
    {
        var snapshots = new List<ItemOrcamento>();
        var catalogoIds = new HashSet<Guid>();
        var linhasIds = new HashSet<Guid>();

        foreach (var item in itens)
        {
            if (item.Quantidade < 1)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Quantidade deve ser maior que zero.");
            if (item.ValorUnitario < 0)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Valor unitario nao pode ser negativo.");

            ItemOrcamento? existente = null;
            if (item.ItemOrcamentoId is Guid itemOrcamentoId)
            {
                if (!linhasIds.Add(itemOrcamentoId))
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("A mesma linha do orcamento foi enviada mais de uma vez.");

                existente = orcamento.Itens.FirstOrDefault(linha => linha.Id == itemOrcamentoId);
                if (existente is null)
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Item nao pertence a este orcamento.");
            }

            if (item.ItemCatalogoOrcamentoId is Guid itemCatalogoId)
            {
                if (!catalogoIds.Add(itemCatalogoId))
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Selecione cada item do catalogo apenas uma vez e ajuste a quantidade.");

                if (existente?.ItemCatalogoOrcamentoId == itemCatalogoId)
                {
                    snapshots.Add(ItemOrcamento.CriarSnapshot(
                        empresaAtual.TenantId,
                        orcamento.Id,
                        existente.ItemCatalogoOrcamentoId,
                        existente.Descricao,
                        existente.Secao,
                        existente.Tipo,
                        item.Quantidade,
                        existente.Tipo == TipoItemOrcamento.ItemMonetario ? item.ValorUnitario : 0m,
                        item.Ordem,
                        existente.NaturezaCobranca));
                    continue;
                }

                var itemCatalogo = await repositorio.ObterItemCatalogoAsync(itemCatalogoId, ct);
                if (itemCatalogo is null || !itemCatalogo.Ativo)
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Item de catalogo ativo nao encontrado.");
                if (itemCatalogo.Tipo == TipoItemOrcamento.ItemMonetario && !itemCatalogo.ValorReferencia.HasValue)
                    return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Configure o valor de referencia do item de catalogo antes de inclui-lo no orcamento.");

                snapshots.Add(ItemOrcamento.CriarSnapshot(
                    empresaAtual.TenantId,
                    orcamento.Id,
                    itemCatalogo.Id,
                    itemCatalogo.Descricao,
                    itemCatalogo.Secao,
                    itemCatalogo.Tipo,
                    item.Quantidade,
                    itemCatalogo.Tipo == TipoItemOrcamento.ItemMonetario ? item.ValorUnitario : 0m,
                    item.Ordem,
                    itemCatalogo.NaturezaCobranca));
                continue;
            }

            if (existente is null || existente.ItemCatalogoOrcamentoId is not null)
                return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("Selecione um item ativo do catalogo para cada nova linha do orcamento.");

            snapshots.Add(ItemOrcamento.CriarSnapshot(
                empresaAtual.TenantId,
                orcamento.Id,
                null,
                existente.Descricao,
                existente.Secao,
                existente.Tipo,
                item.Quantidade,
                existente.Tipo == TipoItemOrcamento.ItemMonetario ? item.ValorUnitario : 0m,
                item.Ordem,
                existente.NaturezaCobranca));
        }

        if (snapshots.Count == 0)
            return ResultadoOperacao<List<ItemOrcamento>>.CriarFalha("O orcamento deve conter ao menos um item.");

        return ResultadoOperacao<List<ItemOrcamento>>.CriarSucesso(snapshots);
    }

    private static bool DiaVencimentoValido(int diaVencimento)
        => diaVencimento is >= 1 and <= 31;

    private static bool NaturezaCobrancaValida(
        TipoItemOrcamento tipo,
        NaturezaCobrancaItemOrcamento natureza)
        => tipo == TipoItemOrcamento.ItemMonetario || natureza == NaturezaCobrancaItemOrcamento.NaoRecorrente;

    private static OrcamentoDto MapearOrcamento(Orcamento orcamento)
        => new(
            orcamento.Id,
            orcamento.ProspectId,
            orcamento.Titulo,
            orcamento.Status.ToString(),
            orcamento.ValorTotal,
            orcamento.DiaVencimento,
            orcamento.Observacoes,
            orcamento.MotivoRecusa,
            orcamento.Itens
                .Select(item => new ItemOrcamentoDto(
                    item.Id,
                    item.ItemCatalogoOrcamentoId,
                    item.Descricao,
                    item.Secao.ToString(),
                    item.Tipo.ToString(),
                    item.NaturezaCobranca.ToString(),
                    item.Quantidade,
                    item.ValorUnitario,
                    item.ValorTotal,
                    item.Ordem))
                .ToList());
}
