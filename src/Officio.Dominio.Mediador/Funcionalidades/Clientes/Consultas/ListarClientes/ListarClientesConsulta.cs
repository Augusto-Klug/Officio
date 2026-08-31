using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ListarClientes;

public record ListarClientesConsulta(string? Busca, bool? Ativo) : IRequest<Resultado<List<ClienteDto>>>;
