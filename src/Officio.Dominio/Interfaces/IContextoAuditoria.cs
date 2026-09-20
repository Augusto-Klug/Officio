namespace Officio.Dominio.Interfaces;

public interface IContextoAuditoria
{
    string? OperacaoAtual { get; }
    Guid? UsuarioIdAtual { get; }

    IDisposable Iniciar(string operacao, Guid? usuarioId);
}
