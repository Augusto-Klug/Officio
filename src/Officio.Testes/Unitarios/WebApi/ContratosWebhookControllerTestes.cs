using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.ProcessarWebhookAutentique;
using Officio.WebApi.Controllers;

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
    public async Task Receber_ComAssinaturaValida_EnviaPayloadAoMediator()
    {
        const string payload = """{"event":"document.finished","document":{"id":"doc-1"}}""";
        var mediator = new MediatorFake();
        var controller = CriarController(payload, CriarAssinatura(payload), mediator);

        var resultado = await controller.Receber(CancellationToken.None);

        Assert.IsType<OkResult>(resultado);
        Assert.Equal(payload, mediator.PayloadRecebido);
    }

    private static ContratosWebhookController CriarController(
        string payload,
        string? assinatura,
        IMediator? mediator = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Autentique:WebhookSecret"] = "segredo-teste"
            })
            .Build();
        var controller = new ContratosWebhookController(mediator ?? new MediatorFake(), configuration);
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

    private sealed class MediatorFake : IMediator
    {
        public string? PayloadRecebido { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ProcessarWebhookAutentiqueComando comando)
                PayloadRecebido = comando.Payload;

            return Task.FromResult((TResponse)(object)Resultado<bool>.CriarSucesso(true));
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => Task.FromResult<object?>(Resultado<bool>.CriarSucesso(true));

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => AsyncEnumerable.Empty<TResponse>();

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => AsyncEnumerable.Empty<object?>();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
