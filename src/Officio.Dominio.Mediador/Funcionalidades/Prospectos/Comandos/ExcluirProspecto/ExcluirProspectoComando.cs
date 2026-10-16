using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.ExcluirProspecto;

public record ExcluirProspectoComando(Guid Id) : IRequest<Resultado<bool>>;
