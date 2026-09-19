using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Officio.WebApi.Controllers;

namespace Officio.Testes.Unitarios.WebApi;

public class AdmissoesControllerRotasTestes
{
    [Theory]
    [InlineData(nameof(AdmissoesController.ObterPorId))]
    [InlineData(nameof(AdmissoesController.Atualizar))]
    [InlineData(nameof(AdmissoesController.Concluir))]
    [InlineData(nameof(AdmissoesController.Cancelar))]
    public void AcaoComId_UsaRestricaoGuid(string nomeMetodo)
    {
        var metodo = typeof(AdmissoesController).GetMethod(nomeMetodo);
        var atributo = Assert.Single(metodo!.GetCustomAttributes(false).OfType<HttpMethodAttribute>());
        Assert.Equal("{id:guid}", atributo.Template);
    }
}
