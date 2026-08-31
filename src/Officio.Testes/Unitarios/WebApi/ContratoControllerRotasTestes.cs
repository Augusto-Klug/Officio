using Microsoft.AspNetCore.Mvc;
using Officio.WebApi.Controllers;
using System.Reflection;

namespace Officio.Testes.Unitarios.WebApi;

public class ContratoControllerRotasTestes
{
    [Fact]
    public void OrcamentosController_ConverterParaContrato_UsaRotaActionComId()
    {
        var metodo = typeof(OrcamentosController).GetMethod(nameof(OrcamentosController.ConverterParaContrato));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpPostAttribute>());

        Assert.Equal("{id:guid}", atributo.Template);
    }

    [Fact]
    public void ContratosController_Listar_UsaRotaAction()
    {
        var metodo = typeof(ContratosController).GetMethod(nameof(ContratosController.Listar));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpGetAttribute>());

        Assert.Null(atributo.Template);
    }

    [Fact]
    public void ContratosController_ObterPorId_UsaRotaActionComId()
    {
        var metodo = typeof(ContratosController).GetMethod(nameof(ContratosController.ObterPorId));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpGetAttribute>());

        Assert.Equal("{id:guid}", atributo.Template);
    }
}
