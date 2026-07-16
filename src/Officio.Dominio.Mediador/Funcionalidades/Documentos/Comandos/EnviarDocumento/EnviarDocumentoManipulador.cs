using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;

internal class EnviarDocumentoManipulador(
    IDocumentoServico documentoServico) : IRequestHandler<EnviarDocumentoComando, Resultado<ResultadoEnvioDocumentoDto>>
{
    public async Task<Resultado<ResultadoEnvioDocumentoDto>> Handle(
        EnviarDocumentoComando request,
        CancellationToken ct)
    {
        var resultado = await documentoServico.EnviarAsync(
            new EnviarDocumentoRequisicao(
                request.Arquivo,
                request.NomeArquivo,
                request.Tipo,
                request.ClientId,
                request.ProspectId,
                request.SocioId,
                request.Observacoes),
            ct);

        return resultado.Sucesso
            ? Resultado<ResultadoEnvioDocumentoDto>.CriarSucesso(
                new ResultadoEnvioDocumentoDto(
                    resultado.Valor!.Id,
                    resultado.Valor.NomeArquivo,
                    resultado.Valor.QualidadeEstimada,
                    resultado.Valor.Legivel,
                    resultado.Valor.Mensagem))
            : Resultado<ResultadoEnvioDocumentoDto>.CriarFalha(resultado.Erro!);
    }
}
