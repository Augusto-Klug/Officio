namespace Officio.Dominio.Contratos.Dtos;

public sealed record ContratoAtivoResumoDto(
    Guid Id,
    string Numero,
    DateTime DataInicio,
    DateTime? DataFim);
