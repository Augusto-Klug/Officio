using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Contratos;

public record PdfContratoResultado(string PdfUrl, DateTime GeradoEm);

public record EnviarAssinaturaRequisicao(
    Guid ContratoId,
    IReadOnlyCollection<SignatarioAssinatura> Signatarios);

public record SignatarioAssinatura(string Nome, string Email, string? Cpf, string Tipo);

public record EnvioAssinaturaResultado(
    string Metodo,
    string? EnvelopeId,
    List<LinkAssinante>? Signatarios,
    string? Mensagem,
    string? PdfUrl);
