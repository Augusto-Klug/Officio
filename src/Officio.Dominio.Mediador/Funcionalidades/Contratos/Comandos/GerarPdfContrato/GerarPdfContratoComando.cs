using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Comandos.GerarPdfContrato;

public record GerarPdfContratoComando(Guid ContractId) : IRequest<Resultado<ResultadoPdfContratoDto>>;

