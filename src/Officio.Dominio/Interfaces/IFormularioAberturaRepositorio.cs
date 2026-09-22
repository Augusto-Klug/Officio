using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IFormularioAberturaRepositorio
{
    Task<Prospecto?> ObterProspectoAsync(Guid prospectoId, CancellationToken ct = default);
    Task<FormularioAbertura?> ObterPorProspectoAsync(Guid prospectoId, CancellationToken ct = default);
    Task<StatusFormulario> ObterOuCriarStatusAsync(string codigo, string descricao, int ordem, CancellationToken ct = default);
    Task AdicionarAsync(FormularioAbertura formulario, CancellationToken ct = default);
    Task RemoverAtividadesAsync(IEnumerable<FormularioAberturaAtividade> atividades, CancellationToken ct = default);
    Task AdicionarAtividadesAsync(IEnumerable<FormularioAberturaAtividade> atividades, CancellationToken ct = default);
    Task SalvarAlteracoesAsync(CancellationToken ct = default);
}
