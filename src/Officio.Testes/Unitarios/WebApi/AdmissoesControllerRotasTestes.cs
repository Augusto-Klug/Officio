using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Officio.WebApi.Controllers;

namespace Officio.Testes.Unitarios.WebApi;

public class AdmissoesControllerRotasTestes
{
    [Fact]
    public void ListarEmpresasComAdmissoes_UsaGet()
    {
        var metodo = typeof(AdmissoesController).GetMethod(nameof(AdmissoesController.ListarEmpresasComAdmissoes));
        Assert.NotNull(metodo);
        Assert.NotEmpty(metodo!.GetCustomAttributes(false).OfType<HttpGetAttribute>());
    }

    [Theory]
    [InlineData(nameof(AdmissoesController.ObterPorId))]
    [InlineData(nameof(AdmissoesController.Atualizar))]
    [InlineData(nameof(AdmissoesController.Concluir))]
    [InlineData(nameof(AdmissoesController.Cancelar))]
    [InlineData(nameof(AdmissoesController.GerarLinkFicha))]
    [InlineData(nameof(AdmissoesController.RevogarLinkFicha))]
    public void AcaoComId_UsaRestricaoGuid(string nomeMetodo)
    {
        var metodo = typeof(AdmissoesController).GetMethod(nomeMetodo);
        var atributo = Assert.Single(metodo!.GetCustomAttributes(false).OfType<HttpMethodAttribute>());
        Assert.Equal("{id:guid}", atributo.Template);
    }

    [Theory]
    [InlineData(nameof(AdmissoesController.ListarDocumentos), "{id:guid}")]
    [InlineData(nameof(AdmissoesController.EnviarDocumento), "{id:guid}/itens/{itemId:guid}")]
    [InlineData(nameof(AdmissoesController.ObterUrlDocumento), "{id:guid}/itens/{itemId:guid}")]
    [InlineData(nameof(AdmissoesController.ValidarDocumento), "{id:guid}/itens/{itemId:guid}")]
    [InlineData(nameof(AdmissoesController.AprovarDocumento), "{id:guid}/itens/{itemId:guid}")]
    [InlineData(nameof(AdmissoesController.RemoverDocumento), "{id:guid}/itens/{itemId:guid}")]
    public void AcaoDocumento_UsaIdsComRestricaoGuid(string nomeMetodo, string rotaEsperada)
    {
        var metodo = typeof(AdmissoesController).GetMethod(nomeMetodo);
        var atributo = Assert.Single(metodo!.GetCustomAttributes(false).OfType<HttpMethodAttribute>());
        Assert.Equal(rotaEsperada, atributo.Template);
    }
}
