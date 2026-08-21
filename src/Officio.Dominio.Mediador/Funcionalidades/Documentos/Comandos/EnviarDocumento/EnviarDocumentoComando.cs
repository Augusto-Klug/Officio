using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;

public record EnviarDocumentoComando(EnviarDocumentoRequisicao Requisicao)
    : IRequest<Resultado<ResultadoEnvioDocumentoDto>>;

public record ResultadoEnvioDocumentoDto(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Message);
