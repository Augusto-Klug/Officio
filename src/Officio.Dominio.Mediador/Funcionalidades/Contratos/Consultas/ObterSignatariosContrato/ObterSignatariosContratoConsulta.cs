using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Contratos.Consultas.ObterSignatariosContrato;

public sealed record ObterSignatariosContratoConsulta(Guid ContratoId)
    : IRequest<Resultado<SignatariosContratoDto>>;
