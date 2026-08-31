using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;

internal class EnviarDocumentoHandler(
    IDocumentoServico documentoServico) : IRequestHandler<EnviarDocumentoComando, Resultado<ResultadoEnvioDocumentoDto>>
{
    public async Task<Resultado<ResultadoEnvioDocumentoDto>> Handle(
        EnviarDocumentoComando request,
        CancellationToken ct)
        => await documentoServico.EnviarAsync(request.Requisicao, ct);
}
