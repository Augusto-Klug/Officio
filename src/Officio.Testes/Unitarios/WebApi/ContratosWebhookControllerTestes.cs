using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Officio.WebApi.Controllers;
using Officio.WebApi.Servicos.Webhooks;

namespace Officio.Testes.Unitarios.WebApi;

public class ContratosWebhookControllerTestes
{
    [Fact]
    public async Task Receber_ComAssinaturaInvalida_RetornaUnauthorized()
    {
        var controller = CriarController("""{"event":"document.finished"}""", "assinatura-invalida");

        var resultado = await controller.Receber(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(resultado);
    }

    [Fact]
    public async Task Receber_ComAssinaturaValida_EnfileiraPayloadERetornaAccepted()
    {
        const string payload = """{"event":"document.finished","document":{"id":"doc-1"}}""";
        var fila = new FilaFake();
        var controller = CriarController(payload, CriarAssinatura(payload), fila);

        var resultado = await controller.Receber(CancellationToken.None);

        Assert.IsType<AcceptedResult>(resultado);
        Assert.Equal(payload, fila.PayloadRecebido);
    }

    [Fact]
    public async Task Receber_ComFilaCheia_RetornaServiceUnavailable()
    {
        const string payload = """{"event":"document.finished","document":{"id":"doc-1"}}""";
        var controller = CriarController(payload, CriarAssinatura(payload), new FilaFake(false));

        var resultado = await controller.Receber(CancellationToken.None);

        var status = Assert.IsType<StatusCodeResult>(resultado);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, status.StatusCode);
    }

    private static ContratosWebhookController CriarController(
        string payload,
        string? assinatura,
        IFilaWebhookAutentique? fila = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Autentique:WebhookSecret"] = "segredo-teste"
            })
            .Build();
        var controller = new ContratosWebhookController(fila ?? new FilaFake(), configuration);
        var contexto = new DefaultHttpContext();
        contexto.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(payload));
        if (assinatura is not null)
            contexto.Request.Headers["x-autentique-signature"] = assinatura;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = contexto
        };

        return controller;
    }

    private static string CriarAssinatura(string payload)
    {
        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes("segredo-teste"),
            Encoding.UTF8.GetBytes(payload));

        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    private sealed class FilaFake(bool aceita = true) : IFilaWebhookAutentique
    {
        public string? PayloadRecebido { get; private set; }

        public bool TentarEnfileirar(string payload)
        {
            if (!aceita)
                return false;

            PayloadRecebido = payload;
            return true;
        }

        public IAsyncEnumerable<string> LerTodosAsync(CancellationToken cancellationToken)
            => AsyncEnumerable.Empty<string>();
    }
}
