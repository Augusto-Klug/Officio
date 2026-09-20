namespace Officio.Dominio.Contratos.Dtos;

public record FaturamentoMensalDto(
    Guid Id,
    Guid ContratoId,
    string NumeroContrato,
    string Cliente,
    DateOnly Competencia,
    string Status,
    int QuantidadeFuncionarios,
    int QuantidadeProLabores,
    int QuantidadeFolhas,
    decimal ValorHonorarioBase,
    decimal ValorPorFolha,
    decimal ValorTotal,
    DateTime CalculadoEm);

public record ResultadoGeracaoFaturamentoDto(
    DateOnly Competencia,
    int Gerados,
    int Ignorados,
    decimal ValorTotal);

public record ResultadoRecalculoFaturamentoDto(
    DateOnly Competencia,
    int Recalculados,
    int Ignorados,
    decimal ValorTotal);

public record ResumoFaturamentoCompetenciaDto(
    DateOnly Competencia,
    int TotalRegistros,
    int Abertos,
    int Fechados,
    int Cancelados,
    int QuantidadeFolhas,
    decimal ValorAberto,
    decimal ValorFechado,
    decimal ValorTotalAtivo);
