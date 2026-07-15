using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Servicos.Modelos;

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
