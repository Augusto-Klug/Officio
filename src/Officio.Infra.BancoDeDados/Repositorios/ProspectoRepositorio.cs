using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;
using Officio.Infra.BancoDeDados.Contextos;

namespace Officio.Infra.BancoDeDados.Repositorios;

internal class ProspectoRepositorio(
    ContextoAplicacao contexto) : RepositorioBase<ContextoAplicacao, Guid>(contexto), IProspectoRepositorio
{
    public async Task<bool> ExisteComCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
        => await VerificarExistenciaAsync<Prospecto>(p => p.Cnpj == cnpj, cancellationToken);

    public async Task<Prospecto?> ObterComSociosAsync(Guid prospectoId, CancellationToken cancellationToken = default)
        => await Contexto.Prospects
            .Include(p => p.Socios)
            .FirstOrDefaultAsync(p => p.Id == prospectoId, cancellationToken);

    public IQueryable<Prospecto> MontarConsulta(bool rastreamento = true)
        => MontarConsulta<Prospecto>(rastreamento);

    public async Task<int> ContarAsync(IQueryable<Prospecto> consulta, CancellationToken cancellationToken = default)
        => await consulta.CountAsync(cancellationToken);

    public List<ProspectoResumo> ObterPagina(IQueryable<Prospecto> consulta, int pagina, int tamanhoPagina)
        => consulta
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(p => new ProspectoResumo(
                p.Id,
                p.RazaoSocial,
                p.Cnpj,
                p.Status.ToString(),
                null,
                p.CriadoEm))
            .ToList();

    public async Task AdicionarAsync(Prospecto prospecto, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Prospecto>(prospecto, cancellationToken);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Cliente>(cliente, cancellationToken);

    public async Task AdicionarSocioAsync(Socio socio, CancellationToken cancellationToken = default)
        => await AdicionarAsync<Socio>(socio, cancellationToken);
}
