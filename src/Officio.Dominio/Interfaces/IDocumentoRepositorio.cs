using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IDocumentoRepositorio
{
    Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken = default);
    Task<Guid?> ObterAdmissaoDocumentoIdAsync(Guid documentoId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> ObterDadosCadastroAsync(Documento documento, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Documento documento, CancellationToken cancellationToken = default);
    Task AdicionarValidacaoAsync(ValidacaoDocumento validacao, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
