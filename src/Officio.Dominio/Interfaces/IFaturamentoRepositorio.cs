using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IFaturamentoRepositorio
{
    Task<List<Contrato>> ListarContratosAptosAsync(DateOnly competencia, CancellationToken ct = default);
    Task<HashSet<Guid>> ListarContratosJaCalculadosAsync(DateOnly competencia, CancellationToken ct = default);
    Task<List<FaturamentoMensal>> ListarAsync(DateOnly? competencia, string? status, CancellationToken ct = default);
    Task<List<FaturamentoMensal>> ListarParaRecalculoAsync(DateOnly competencia, CancellationToken ct = default);
    Task<FaturamentoMensal?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<StatusFaturamento> ObterOuCriarStatusAsync(string codigo, string descricao, CancellationToken ct = default);
    Task AdicionarAsync(FaturamentoMensal faturamento, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
