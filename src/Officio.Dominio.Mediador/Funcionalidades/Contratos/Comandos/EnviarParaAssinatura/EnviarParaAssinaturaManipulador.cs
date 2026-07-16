using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.EnviarParaAssinatura;

internal class EnviarParaAssinaturaManipulador(
    IContratoServico contratoServico) : IRequestHandler<EnviarParaAssinaturaComando, Resultado<SendForSignatureResultDto>>
{
    public async Task<Resultado<SendForSignatureResultDto>> Handle(
        EnviarParaAssinaturaComando request,
        CancellationToken ct)
    {
        var resultado = await contratoServico.EnviarParaAssinaturaAsync(
            new EnviarAssinaturaRequisicao(
                request.ContractId,
                request.Signatarios
                    .Select(s => new SignatarioAssinatura(s.Nome, s.Email, s.Cpf, s.Tipo))
                    .ToList()),
            ct);

        return resultado.Sucesso
            ? Resultado<SendForSignatureResultDto>.CriarSucesso(
                new SendForSignatureResultDto(
                    resultado.Valor!.Metodo,
                    resultado.Valor.EnvelopeId,
                    resultado.Valor.Signatarios,
                    resultado.Valor.Mensagem,
                    resultado.Valor.PdfUrl))
            : Resultado<SendForSignatureResultDto>.CriarFalha(resultado.Erro!);
    }
}
