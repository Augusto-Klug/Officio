using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

public record EnviarParaAssinaturaComando(
    Guid ContractId,
    List<SignatarioDto> Signatarios,
    string? MensagemPersonalizada = null) : IRequest<Resultado<SendForSignatureResultDto>>;

public record SignatarioDto(string Nome, string Email, string? Cpf, string Tipo);

public record SendForSignatureResultDto(
    string Metodo,
    string? DsignEnvelopeId,
    List<LinkAssinante>? Signatarios,
    string? Message,
    string? PdfUrl);





