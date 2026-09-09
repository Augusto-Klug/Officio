using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterPdfAssinadoContrato;

public record ObterPdfAssinadoContratoConsulta(Guid ContratoId)
    : IRequest<Resultado<ResultadoPdfAssinadoContratoDto>>;
