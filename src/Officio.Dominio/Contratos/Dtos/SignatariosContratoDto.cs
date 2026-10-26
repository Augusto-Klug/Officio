namespace Officio.Dominio.Contratos.Dtos;

public sealed record SignatarioContratoDto(
    string Tipo,
    string Nome,
    string Cpf,
    string? Email);

public sealed record SignatariosContratoDto(
    IReadOnlyList<SignatarioContratoDto> Signatarios);

public sealed record AssinaturaSignatarioResumoDto(
    string Tipo,
    string Nome,
    bool Assinado,
    DateTime? AssinadoEm);
