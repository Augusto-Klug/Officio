using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class FormularioAberturaRepositorio(ContextoAplicacao contexto) : IFormularioAberturaRepositorio
{
    public Task<Prospecto?> ObterProspectoAsync(Guid prospectoId, CancellationToken ct = default)
        => contexto.Prospects.FirstOrDefaultAsync(item => item.Id == prospectoId, ct);

    public Task<FormularioAbertura?> ObterPorProspectoAsync(Guid prospectoId, CancellationToken ct = default)
        => contexto.FormulariosAbertura
            .Include(item => item.StatusFormulario)
            .Include(item => item.Atividades)
            .FirstOrDefaultAsync(item => item.ProspectId == prospectoId, ct);

    public async Task<StatusFormulario> ObterOuCriarStatusAsync(
        string codigo,
        string descricao,
        int ordem,
        CancellationToken ct = default)
    {
        var statusLocal = contexto.StatusFormularios.Local.FirstOrDefault(item => item.Codigo == codigo);
        if (statusLocal is not null) return statusLocal;

        var status = await contexto.StatusFormularios.FirstOrDefaultAsync(item => item.Codigo == codigo, ct);
        if (status is not null) return status;

        status = new StatusFormulario(codigo, descricao, ordem);
        await contexto.StatusFormularios.AddAsync(status, ct);
        return status;
    }

    public Task AdicionarAsync(FormularioAbertura formulario, CancellationToken ct = default)
        => contexto.FormulariosAbertura.AddAsync(formulario, ct).AsTask();

    public Task RemoverAtividadesAsync(IEnumerable<FormularioAberturaAtividade> atividades, CancellationToken ct = default)
    {
        contexto.FormularioAberturaAtividades.RemoveRange(atividades);
        return Task.CompletedTask;
    }

    public Task AdicionarAtividadesAsync(IEnumerable<FormularioAberturaAtividade> atividades, CancellationToken ct = default)
        => contexto.FormularioAberturaAtividades.AddRangeAsync(atividades, ct);

    public Task SalvarAlteracoesAsync(CancellationToken ct = default) => contexto.SaveChangesAsync(ct);
}
