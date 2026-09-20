namespace Officio.Dominio.Contratos.Dtos;

public record DashboardVisaoGeralDto(
    int ProspectosEmAberto,
    int ClientesAtivos,
    int ContratosAtivos,
    int AdmissoesPendentes,
    int OrcamentosAguardandoRetorno,
    decimal ReceitaMensalContratada);

public record DashboardAlertaDto(
    string Codigo,
    string Severidade,
    string Titulo,
    string Descricao,
    Guid ReferenciaId,
    string Rota,
    DateTime DataReferencia);

public record DashboardProjecaoReceitaDto(
    DateOnly Mes,
    decimal ReceitaContratada);

public record DashboardContratoReceitaDto(
    DateTime DataInicio,
    DateTime? DataFim,
    decimal ReceitaMensal);
