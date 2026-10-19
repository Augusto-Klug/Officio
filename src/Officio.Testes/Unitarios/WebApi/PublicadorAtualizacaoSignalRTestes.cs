using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Officio.Dominio.Eventos;
using Officio.WebApi.TempoReal;

namespace Officio.Testes.Unitarios.WebApi;

public class PublicadorAtualizacaoSignalRTestes
{
    [Fact]
    public async Task PublicarAsync_EnviaInvalidacaoAoGrupoDoParceiroSemDadosDeDominio()
    {
        var hubContext = new HubContextFake();
        var publicador = new PublicadorAtualizacaoSignalR(
            hubContext,
            NullLogger<PublicadorAtualizacaoSignalR>.Instance);
        var evento = new EventoAtualizacaoTempoReal(
            Guid.Parse("365021a1-0ad4-4b2e-95ce-0d46d817ad3d"),
            EntidadesAtualizacaoTempoReal.Admissao,
            Guid.Parse("4ec1bcbb-9581-44ba-a70a-b3a6c3cfdf98"));

        await publicador.PublicarAsync(evento);

        Assert.Equal("parceiro:365021a10ad44b2e95ce0d46d817ad3d", hubContext.ClientsFake.GrupoSelecionado);
        var mensagem = Assert.IsType<AtualizacaoTempoRealMensagem>(
            Assert.Single(hubContext.ClientsFake.Proxy.Mensagens).Argumentos[0]);
        Assert.Equal("Atualizacao", Assert.Single(hubContext.ClientsFake.Proxy.Mensagens).Metodo);
        Assert.Equal(EntidadesAtualizacaoTempoReal.Admissao, mensagem.Entidade);
        Assert.Equal(evento.Id, mensagem.Id);
    }

    private sealed class HubContextFake : IHubContext<AtualizacoesHub>
    {
        public HubClientsFake ClientsFake { get; } = new();
        public IHubClients Clients => ClientsFake;
        public IGroupManager Groups => throw new NotSupportedException();
    }

    private sealed class HubClientsFake : IHubClients
    {
        public string? GrupoSelecionado { get; private set; }
        public ClientProxyFake Proxy { get; } = new();

        public IClientProxy Group(string groupName)
        {
            GrupoSelecionado = groupName;
            return Proxy;
        }

        public IClientProxy All => throw new NotSupportedException();
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public IClientProxy Client(string connectionId) => throw new NotSupportedException();
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
        public IClientProxy User(string userId) => throw new NotSupportedException();
        public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
    }

    private sealed class ClientProxyFake : IClientProxy
    {
        public List<(string Metodo, object?[] Argumentos)> Mensagens { get; } = [];

        public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        {
            Mensagens.Add((method, args));
            return Task.CompletedTask;
        }
    }
}
