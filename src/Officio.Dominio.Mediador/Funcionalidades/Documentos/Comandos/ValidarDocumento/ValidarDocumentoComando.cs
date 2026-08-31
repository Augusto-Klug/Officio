using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;

public record ValidarDocumentoComando(
    Guid DocumentId) : IRequest<Resultado<ValidationResultDto>>;

