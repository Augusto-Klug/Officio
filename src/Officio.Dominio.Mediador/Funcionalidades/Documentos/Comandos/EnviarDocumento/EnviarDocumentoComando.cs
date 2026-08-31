using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;

public record EnviarDocumentoComando(EnviarDocumentoRequisicao Requisicao)
    : IRequest<Resultado<ResultadoEnvioDocumentoDto>>;
