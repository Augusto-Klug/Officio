using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Documentos.Comandos.ValidarDocumento;

public record ValidarDocumentoComando(
    Guid DocumentId) : IRequest<Resultado<ValidationResultDto>>;

public record ValidationResultDto(
    bool Valido,
    decimal ConfiancaOcr,
    List<DivergenciaDto> Divergencias,
    bool FallbackManual,
    string? Message);

public record DivergenciaDto(
    string Campo,
    string ValorDocumento,
    string ValorCadastro,
    decimal Similaridade);





