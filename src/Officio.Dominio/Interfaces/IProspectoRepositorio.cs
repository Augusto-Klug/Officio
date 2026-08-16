using Officio.Dominio.Contratos;
using Officio.Dominio.Entidades;

namespace Officio.Dominio.Interfaces;

public interface IProspectoRepositorio
{
    Task<bool> ExisteComCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
    Task<Prospecto?> ObterComSociosAsync(Guid prospectoId, CancellationToken cancellationToken = default);
    IQueryable<Prospecto> MontarConsulta(bool rastreamento = true);
    Task<int> ContarAsync(IQueryable<Prospecto> consulta, CancellationToken cancellationToken = default);
    List<ProspectoResumo> ObterPagina(IQueryable<Prospecto> consulta, int pagina, int tamanhoPagina);
    Task AdicionarAsync(Prospecto prospecto, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AdicionarSocioAsync(Socio socio, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
