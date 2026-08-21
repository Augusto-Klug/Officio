using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

public record EnviarParaAssinaturaComando(EnviarAssinaturaRequisicao Requisicao)
    : IRequest<Resultado<SendForSignatureResultDto>>;

public record SignatarioDto(string Nome, string Email, string? Cpf, string Tipo);

public record SendForSignatureResultDto(
    string Metodo,
    string? DsignEnvelopeId,
    List<LinkAssinante>? Signatarios,
    string? Message,
    string? PdfUrl);
