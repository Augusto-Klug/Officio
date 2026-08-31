using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;

internal class ValidarDocumentoHandler(
    IDocumentoServico documentoServico) : IRequestHandler<ValidarDocumentoComando, Resultado<ValidationResultDto>>
{
    public async Task<Resultado<ValidationResultDto>> Handle(
        ValidarDocumentoComando request,
        CancellationToken ct)
        => await documentoServico.ValidarAsync(request.DocumentId, ct);
}
