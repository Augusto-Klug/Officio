using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Officio.Infraestrutura.Servicos;

namespace Officio.Testes.Unitarios.Infraestrutura;

public class ServicoEmailTestes
{
    [Fact]
    public async Task ConteudoNoLog_SuprimeEnvioSemRegistrarCorpoOuToken()
    {
        var logger = new LoggerFake();
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:ExporConteudoNoLog"] = "true"
            })
            .Build();
        var servico = new ServicoEmail(logger, configuracao);
        const string tokenPrivado = "token-de-convite-privado";

        await servico.SendAsync(
            "destinatario@example.test",
            "Convite para acessar o Officio",
            $"Acesse https://localhost/ativar?token={tokenPrivado}");

        var log = Assert.Single(logger.Mensagens);
        Assert.Contains("Envio de e-mail suprimido", log);
        Assert.DoesNotContain(tokenPrivado, log);
        Assert.DoesNotContain("destinatario@example.test", log);
    }

    private sealed class LoggerFake : ILogger<ServicoEmail>
    {
        public List<string> Mensagens { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Mensagens.Add(formatter(state, exception));
    }
}
