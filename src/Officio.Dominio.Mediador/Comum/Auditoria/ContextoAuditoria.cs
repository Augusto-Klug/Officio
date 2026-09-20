using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Comum.Auditoria;

internal sealed class ContextoAuditoria : IContextoAuditoria
{
    private readonly Stack<OperacaoAuditoria> operacoes = new();

    public string? OperacaoAtual => operacoes.TryPeek(out var operacao) ? operacao.Nome : null;
    public Guid? UsuarioIdAtual => operacoes.TryPeek(out var operacao) ? operacao.UsuarioId : null;

    public IDisposable Iniciar(string operacao, Guid? usuarioId)
    {
        operacoes.Push(new OperacaoAuditoria(operacao, usuarioId));
        return new EscopoAuditoria(operacoes);
    }

    private sealed record OperacaoAuditoria(string Nome, Guid? UsuarioId);

    private sealed class EscopoAuditoria(Stack<OperacaoAuditoria> operacoes) : IDisposable
    {
        private bool finalizado;

        public void Dispose()
        {
            if (finalizado)
                return;

            finalizado = true;
            operacoes.Pop();
        }
    }
}
