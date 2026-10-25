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
    int DuracaoAnos,
    DateOnly? CompetenciaInicialHonorarios,
    bool ConfirmarSubstituicaoContratoAtivo,
    string? ClausulasExtras,
    Guid? RepresentanteSocioId = null);

public record RepresentanteDisponivelDto(Guid Id, string Nome, string? Cpf, string? TipoRelacao,
    bool CadastroCompleto, IReadOnlyList<string> InformacoesFaltantes);
public record RepresentantesContratoDto(Guid ProspectoId, List<RepresentanteDisponivelDto> Socios);

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
    string? ClausulasExtras,
    bool PdfGerado);
