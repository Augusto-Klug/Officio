using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ListarClientes;

public class ListarClientesHandler(IClienteServico clienteServico)
    : IRequestHandler<ListarClientesConsulta, Resultado<List<ClienteDto>>>
{
    public async Task<Resultado<List<ClienteDto>>> Handle(ListarClientesConsulta request, CancellationToken ct)
        => await clienteServico.ListarAsync(request.Busca, request.Ativo, ct);
}
