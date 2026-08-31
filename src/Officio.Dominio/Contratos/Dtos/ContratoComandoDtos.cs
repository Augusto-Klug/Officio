using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Contratos.Dtos;

public record ResultadoPdfContratoDto(string PdfUrl, DateTime GeradoEm);

public record SignatarioDto(string Nome, string Email, string? Cpf, string Tipo);

public record SendForSignatureResultDto(
    string Metodo,
    string? DsignEnvelopeId,
    List<LinkAssinante>? Signatarios,
    string? Message,
    string? PdfUrl);
