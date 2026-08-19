using Officio.Dominio.Enumeradores;
using Microsoft.AspNetCore.Mvc;
using Officio.ViewModels.Enumeradores;
using Officio.ViewModels.Orcamentos;
using Officio.WebApi.Controllers;
using Officio.WebApi.Conversores;

namespace Officio.Testes.Unitarios.WebApi;

public class OrcamentoViewModelConversorTestes
{
    [Fact]
    public void CriarOrcamentoViewModel_ParaComando_MapeiaItens()
    {
        var prospectoId = Guid.Parse("b29a9417-2109-4bc7-a6c3-d0a461965fab");
        var itemCatalogoId = Guid.Parse("c3f836ec-ae27-408d-a319-557e1a661678");
        var viewModel = new CriarOrcamentoViewModel
        {
            ProspectoId = prospectoId,
            Titulo = "Orcamento mensal",
            DiaVencimento = 15,
            Observacoes = "Vigente por trinta dias",
            Itens =
            [
                new CriarItemOrcamentoViewModel
                {
                    ItemCatalogoOrcamentoId = itemCatalogoId,
                    Descricao = "Escrituracao contabil",
                    Secao = SecaoItemOrcamentoViewModel.ServicosInclusos,
                    Tipo = TipoItemOrcamentoViewModel.ItemMonetario,
                    Quantidade = 2,
                    ValorUnitario = 199.90m,
                    Ordem = 3
                }
            ]
        };

        var comando = viewModel.ParaComando();

        Assert.Equal(prospectoId, comando.ProspectoId);
        Assert.Equal("Orcamento mensal", comando.Titulo);
        var item = Assert.Single(comando.Itens);
        Assert.Equal(itemCatalogoId, item.ItemCatalogoOrcamentoId);
        Assert.Equal("Escrituracao contabil", item.Descricao);
        Assert.Equal(SecaoItemOrcamento.ServicosInclusos, item.Secao);
        Assert.Equal(TipoItemOrcamento.ItemMonetario, item.Tipo);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(199.90m, item.ValorUnitario);
        Assert.Equal(3, item.Ordem);
    }

    [Fact]
    public void CriarItemCatalogoViewModel_ParaComando_MapeiaEnumsEValor()
    {
        var viewModel = new CriarItemCatalogoOrcamentoViewModel
        {
            Descricao = "Abertura de empresa",
            Secao = SecaoItemOrcamentoViewModel.CriacaoEmpresa,
            Tipo = TipoItemOrcamentoViewModel.RegraComercial,
            ValorReferencia = 1200.50m
        };

        var comando = viewModel.ParaComando();

        Assert.Equal("Abertura de empresa", comando.Descricao);
        Assert.Equal(SecaoItemOrcamento.CriacaoEmpresa, comando.Secao);
        Assert.Equal(TipoItemOrcamento.RegraComercial, comando.Tipo);
        Assert.Equal(1200.50m, comando.ValorReferencia);
    }

    [Fact]
    public async Task AlterarStatus_QuandoStatusInvalido_RetornaBadRequest()
    {
        var controlador = new OrcamentosControlador(null!);
        var viewModel = new AlterarStatusOrcamentoViewModel();

        var resultado = await controlador.AlterarStatus(Guid.NewGuid(), viewModel, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Status do orcamento invalido.", badRequest.Value);
    }

    [Fact]
    public async Task CriarItemCatalogo_QuandoSecaoInvalida_RetornaBadRequest()
    {
        var controlador = new OrcamentosControlador(null!);
        var viewModel = new CriarItemCatalogoOrcamentoViewModel
        {
            Descricao = "Servico",
            Tipo = TipoItemOrcamentoViewModel.ItemMonetario
        };

        var resultado = await controlador.CriarItemCatalogo(viewModel, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Secao do item de orcamento invalida.", badRequest.Value);
    }

    [Fact]
    public async Task CriarItemCatalogo_QuandoTipoInvalido_RetornaBadRequest()
    {
        var controlador = new OrcamentosControlador(null!);
        var viewModel = new CriarItemCatalogoOrcamentoViewModel
        {
            Descricao = "Servico",
            Secao = SecaoItemOrcamentoViewModel.Honorarios
        };

        var resultado = await controlador.CriarItemCatalogo(viewModel, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("Tipo do item de orcamento invalido.", badRequest.Value);
    }
}
