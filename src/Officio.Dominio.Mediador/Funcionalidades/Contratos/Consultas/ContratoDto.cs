namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas;

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
    bool RenovacaoAutomatica,
    string? ClausulasExtras,
    bool PdfGerado);
