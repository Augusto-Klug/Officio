using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.FormulariosAbertura.Consultas.ObterPorProspecto;

public record ObterFormularioAberturaPorProspectoConsulta(Guid ProspectoId)
    : IRequest<Resultado<FormularioAberturaDto>>;
