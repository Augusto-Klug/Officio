using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ObterClientePorId;

public record ObterClientePorIdConsulta(Guid ClienteId) : IRequest<Resultado<ClienteDetalheDto>>;
