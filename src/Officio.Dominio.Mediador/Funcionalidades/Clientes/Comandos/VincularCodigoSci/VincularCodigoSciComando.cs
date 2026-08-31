using MediatR;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.VincularCodigoSci;

public record VincularCodigoSciComando(Guid ClienteId, string SciCodigo) : IRequest<Resultado<bool>>;
