using Microsoft.AspNetCore.Mvc;
using Officio.WebApi.Controllers;
using System.Reflection;

namespace Officio.Testes.Unitarios.WebApi;

public class ContratoControllerRotasTestes
{
    [Fact]
    public void OrcamentosControlador_ConverterParaContrato_UsaRotaActionComId()
    {
        var metodo = typeof(OrcamentosControlador).GetMethod(nameof(OrcamentosControlador.ConverterParaContrato));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpPostAttribute>());

        Assert.Equal("{id:guid}", atributo.Template);
    }

    [Fact]
    public void ContratosControlador_Listar_UsaRotaAction()
    {
        var metodo = typeof(ContratosControlador).GetMethod(nameof(ContratosControlador.Listar));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpGetAttribute>());

        Assert.Null(atributo.Template);
    }

    [Fact]
    public void ContratosControlador_ObterPorId_UsaRotaActionComId()
    {
        var metodo = typeof(ContratosControlador).GetMethod(nameof(ContratosControlador.ObterPorId));

        var atributo = Assert.Single(metodo!.GetCustomAttributes<HttpGetAttribute>());

        Assert.Equal("{id:guid}", atributo.Template);
    }
}
