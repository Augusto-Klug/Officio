using MediatR;
using Officio.Dominio.Mediador.Comum;
using Officio.Dominio.Servicos.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;

public class ValidarDocumentoManipulador(
    IDocumentoServico documentoServico) : IRequestHandler<ValidarDocumentoComando, Resultado<ValidationResultDto>>
{
    public async Task<Resultado<ValidationResultDto>> Handle(
        ValidarDocumentoComando request,
        CancellationToken ct)
    {
        var resultado = await documentoServico.ValidarAsync(request.DocumentId, ct);

        if (!resultado.Sucesso)
            return Resultado<ValidationResultDto>.CriarFalha(resultado.Erro!);

        var validacao = resultado.Valor!;

        return Resultado<ValidationResultDto>.CriarSucesso(
            new ValidationResultDto(
                validacao.Valido,
                validacao.ConfiancaOcr,
                validacao.Divergencias
                    .Select(d => new DivergenciaDto(
                        d.Campo,
                        d.ValorDocumento,
                        d.ValorCadastro,
                        d.Similaridade))
                    .ToList(),
                validacao.FallbackManual,
                validacao.Mensagem));
    }
}
