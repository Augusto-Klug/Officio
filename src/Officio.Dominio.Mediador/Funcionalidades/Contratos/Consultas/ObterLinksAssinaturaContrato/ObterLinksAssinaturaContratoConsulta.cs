using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterLinksAssinaturaContrato;

public sealed record ObterLinksAssinaturaContratoConsulta(Guid ContratoId)
    : IRequest<Resultado<LinksAssinaturaContratoDto>>;
