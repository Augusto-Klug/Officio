using MediatR;
using Officio.Dominio.Mediador.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;

public record GerarPdfContratoComando(Guid ContractId) : IRequest<Resultado<ResultadoPdfContratoDto>>;

public record ResultadoPdfContratoDto(string PdfUrl, DateTime GeradoEm);





