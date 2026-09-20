using MediatR;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum.Comportamentos;

namespace Officio.Testes.Unitarios.DominioMediador;

public class ComportamentoAuditoriaTestes
{
    [Fact]
    public async Task Handle_QuandoRequisicaoEhComando_DisponibilizaOperacaoEUsuarioDuranteExecucao()
    {
        var usuarioId = Guid.NewGuid();
        var contexto = new ContextoAuditoriaFake();
        var comportamento = new ComportamentoAuditoria<TesteComando, string>(
            contexto,
            new UsuarioAtualFake(usuarioId));

        var resposta = await comportamento.Handle(
            new TesteComando(),
            _ =>
            {
                Assert.Equal(nameof(TesteComando), contexto.OperacaoAtual);
                Assert.Equal(usuarioId, contexto.UsuarioIdAtual);
                return Task.FromResult("executado");
            },
            CancellationToken.None);

        Assert.Equal("executado", resposta);
        Assert.Null(contexto.OperacaoAtual);
        Assert.Null(contexto.UsuarioIdAtual);
    }

    [Fact]
    public async Task Handle_QuandoRequisicaoEhConsulta_NaoIniciaAuditoria()
    {
        var contexto = new ContextoAuditoriaFake();
        var comportamento = new ComportamentoAuditoria<ConsultaTeste, string>(
            contexto,
            new UsuarioAtualFake(Guid.NewGuid()));

        await comportamento.Handle(
            new ConsultaTeste(),
            _ =>
            {
                Assert.Null(contexto.OperacaoAtual);
                return Task.FromResult("consultado");
            },
            CancellationToken.None);

        Assert.Equal(0, contexto.QuantidadeInicios);
    }

    private sealed record TesteComando : IRequest<string>;
    private sealed record ConsultaTeste : IRequest<string>;

    private sealed class UsuarioAtualFake(Guid usuarioId) : IUsuarioAtual
    {
        public Guid UserId => usuarioId;
        public string? Email => null;
    }

    private sealed class ContextoAuditoriaFake : IContextoAuditoria
    {
        public string? OperacaoAtual { get; private set; }
        public Guid? UsuarioIdAtual { get; private set; }
        public int QuantidadeInicios { get; private set; }

        public IDisposable Iniciar(string operacao, Guid? usuarioId)
        {
            QuantidadeInicios++;
            OperacaoAtual = operacao;
            UsuarioIdAtual = usuarioId;
            return new Escopo(() =>
            {
                OperacaoAtual = null;
                UsuarioIdAtual = null;
            });
        }

        private sealed class Escopo(Action finalizar) : IDisposable
        {
            public void Dispose() => finalizar();
        }
    }
}
