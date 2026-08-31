using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Contratos;

public record PdfContratoResultado(string PdfUrl, DateTime GeradoEm);

public record EnviarAssinaturaRequisicao(
    Guid ContratoId,
    IReadOnlyCollection<SignatarioAssinatura> Signatarios,
    string? MensagemPersonalizada = null);

public record SignatarioAssinatura(string Nome, string Email, string? Cpf, string Tipo);

public record EnvioAssinaturaResultado(
    string Metodo,
    string? EnvelopeId,
    List<LinkAssinante>? Signatarios,
    string? Mensagem,
    string? PdfUrl);

public record ConverterOrcamentoContratoRequisicao(
    Guid OrcamentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    bool RenovacaoAutomatica,
    string? ClausulasExtras);

public record ContratoResultado(
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
