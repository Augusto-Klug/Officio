using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

internal class EnviarParaAssinaturaHandler(
    IContratoServico contratoServico) : IRequestHandler<EnviarParaAssinaturaComando, Resultado<SendForSignatureResultDto>>
{
    public async Task<Resultado<SendForSignatureResultDto>> Handle(
        EnviarParaAssinaturaComando request,
        CancellationToken ct)
        => await contratoServico.EnviarParaAssinaturaAsync(request.Requisicao, ct);
}
