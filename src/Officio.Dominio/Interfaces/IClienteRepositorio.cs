using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IClienteRepositorio
{
    Task<List<Cliente>> ListarAsync(string? busca = null, bool? ativo = null, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterDetalheAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
