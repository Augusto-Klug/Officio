using MediatR;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.EnviarDocumento;

public record EnviarDocumentoComando(
    Stream Arquivo,
    string NomeArquivo,
    TipoDocumento Tipo,
    Guid? ClientId,
    Guid? ProspectId,
    Guid? SocioId,
    string? Observacoes) : IRequest<Resultado<ResultadoEnvioDocumentoDto>>;

public record ResultadoEnvioDocumentoDto(
    Guid Id,
    string NomeArquivo,
    decimal QualidadeEstimada,
    bool Legivel,
    string Message);





