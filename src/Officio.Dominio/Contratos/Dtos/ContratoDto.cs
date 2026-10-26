namespace Officio.Dominio.Contratos.Dtos;

public record ContratoDto(
    Guid Id,
    string Numero,
    Guid ProspectoId,
    Guid? ClienteId,
    Guid? OrcamentoId,
    string RazaoSocial,
    string Status,
    decimal ValorTotal,
    int DiaVencimento,
    DateTime DataInicio,
    DateTime? DataFim,
    string? ClausulasExtras,
    bool PdfGerado,
    bool PdfAssinadoDisponivel,
    DateTime? AssinadoEm,
    Guid? RepresentanteSocioId = null,
    int? DuracaoAnos = null,
    DateOnly? CompetenciaInicialHonorarios = null,
    decimal? ValorHonorarioMensal = null,
    decimal? ValorUnitarioPorFolha = null,
    IReadOnlyList<AssinaturaSignatarioResumoDto>? SignatariosAssinatura = null);
