using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Officio.WebApi.Controllers;

namespace Officio.Testes.Unitarios.WebApi;

public class NotificacoesControllerRotasTestes
{
    [Fact]
    public void Controller_ExigeAutenticacaoEUsaRotaDeNotificacoes()
    {
        var tipo = typeof(NotificacoesController);

        Assert.NotNull(tipo.GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
        var rota = Assert.Single(tipo.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>());
        Assert.Equal("api/v1/notificacoes/[action]", rota.Template);
    }

    [Fact]
    public void MarcarComoLida_RecebeIdNaRota()
    {
        var metodo = typeof(NotificacoesController).GetMethod(nameof(NotificacoesController.MarcarComoLida))!;
        var atributo = Assert.Single(metodo.GetCustomAttributes(typeof(HttpPatchAttribute), true).Cast<HttpPatchAttribute>());

        Assert.Equal("{id:guid}", atributo.Template);
    }

    [Fact]
    public void MarcarTodasComoLidas_NaoRecebeIdNaRota()
    {
        var metodo = typeof(NotificacoesController).GetMethod(nameof(NotificacoesController.MarcarTodasComoLidas))!;
        var atributo = Assert.Single(metodo.GetCustomAttributes(typeof(HttpPatchAttribute), true).Cast<HttpPatchAttribute>());

        Assert.Null(atributo.Template);
    }
}
