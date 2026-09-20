using Officio.Dominio.Contratos.Dtos;

namespace Officio.Dominio.Interfaces;

public interface IDashboardRepositorio
{
    Task<DashboardVisaoGeralDto> ObterVisaoGeralAsync(DateOnly competencia, CancellationToken cancellationToken = default);
    Task<List<DashboardAlertaDto>> ListarAlertasAsync(
        DateTime inicioContratos,
        DateTime limiteContratos,
        DateTime limiteProspectosSemRetorno,
        int limite,
        CancellationToken cancellationToken = default);
    Task<List<DashboardContratoReceitaDto>> ListarContratosParaProjecaoAsync(
        DateTime inicio,
        DateTime fim,
        CancellationToken cancellationToken = default);
}
