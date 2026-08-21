using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AlterarStatusOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.AtualizarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.CriarOrcamento;
using Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Comandos.RecusarOrcamento;
using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Orcamentos;

namespace Officio.WebApi.Conversores;

internal static class OrcamentoViewModelConversor
{
    public static CriarOrcamentoComando ParaComando(this CriarOrcamentoViewModel viewModel)
        => new(
            viewModel.ProspectoId,
            viewModel.Titulo,
            viewModel.DiaVencimento,
            viewModel.Observacoes,
            ConverterItens(viewModel.Itens));

    public static AtualizarOrcamentoComando ParaComando(this AtualizarOrcamentoViewModel viewModel, Guid id)
        => new(
            id,
            viewModel.Titulo,
            viewModel.DiaVencimento,
            viewModel.Observacoes,
            ConverterItens(viewModel.Itens));

    public static AlterarStatusOrcamentoComando ParaComando(this AlterarStatusOrcamentoViewModel viewModel, Guid id)
        => new(id, Converter(viewModel.Status));

    public static RecusarOrcamentoComando ParaComando(this RecusarOrcamentoViewModel viewModel, Guid id)
        => new(id, viewModel.Motivo);

    public static CriarItemCatalogoOrcamentoComando ParaComando(this CriarItemCatalogoOrcamentoViewModel viewModel)
        => new(
            viewModel.Descricao,
            Converter(viewModel.Secao),
            Converter(viewModel.Tipo),
            viewModel.ValorReferencia);

    public static AtualizarItemCatalogoOrcamentoComando ParaComando(
        this AtualizarItemCatalogoOrcamentoViewModel viewModel,
        Guid id)
        => new(
            id,
            viewModel.Descricao,
            Converter(viewModel.Secao),
            Converter(viewModel.Tipo),
            viewModel.ValorReferencia);

    public static AlterarAtivoItemCatalogoOrcamentoComando ParaComando(
        this AlterarAtivoItemCatalogoOrcamentoViewModel viewModel,
        Guid id)
        => new(id, viewModel.Ativo);

    private static List<CriarItemOrcamentoDto> ConverterItens(IEnumerable<CriarItemOrcamentoViewModel> itens)
        => itens.Select(item => new CriarItemOrcamentoDto(
            item.ItemCatalogoOrcamentoId,
            item.Descricao,
            Converter(item.Secao),
            Converter(item.Tipo),
            item.Quantidade,
            item.ValorUnitario,
            item.Ordem)).ToList();

    private static SecaoItemOrcamento? Converter(SecaoItemOrcamentoViewModel? secao)
        => secao is null ? null : Converter(secao.Value);

    private static SecaoItemOrcamento Converter(SecaoItemOrcamentoViewModel secao)
        => secao switch
        {
            SecaoItemOrcamentoViewModel.ServicosInclusos => SecaoItemOrcamento.ServicosInclusos,
            SecaoItemOrcamentoViewModel.Honorarios => SecaoItemOrcamento.Honorarios,
            SecaoItemOrcamentoViewModel.ServicosSeparados => SecaoItemOrcamento.ServicosSeparados,
            SecaoItemOrcamentoViewModel.CriacaoEmpresa => SecaoItemOrcamento.CriacaoEmpresa,
            _ => throw new ArgumentOutOfRangeException(nameof(secao), secao, null)
        };

    private static TipoItemOrcamento? Converter(TipoItemOrcamentoViewModel? tipo)
        => tipo is null ? null : Converter(tipo.Value);

    private static TipoItemOrcamento Converter(TipoItemOrcamentoViewModel tipo)
        => tipo switch
        {
            TipoItemOrcamentoViewModel.ServicoDescricao => TipoItemOrcamento.ServicoDescricao,
            TipoItemOrcamentoViewModel.ItemMonetario => TipoItemOrcamento.ItemMonetario,
            TipoItemOrcamentoViewModel.RegraComercial => TipoItemOrcamento.RegraComercial,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
        };

    private static StatusOrcamento Converter(StatusOrcamentoViewModel status)
        => status switch
        {
            StatusOrcamentoViewModel.Rascunho => StatusOrcamento.Rascunho,
            StatusOrcamentoViewModel.EmRevisao => StatusOrcamento.EmRevisao,
            StatusOrcamentoViewModel.ProntoParaEnvio => StatusOrcamento.ProntoParaEnvio,
            StatusOrcamentoViewModel.Enviado => StatusOrcamento.Enviado,
            StatusOrcamentoViewModel.Aceito => StatusOrcamento.Aceito,
            StatusOrcamentoViewModel.Recusado => StatusOrcamento.Recusado,
            StatusOrcamentoViewModel.Expirado => StatusOrcamento.Expirado,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
}
