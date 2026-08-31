using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Consultas.ObterClientePorId;

public class ObterClientePorIdHandler(IClienteServico clienteServico)
    : IRequestHandler<ObterClientePorIdConsulta, Resultado<ClienteDetalheDto>>
{
    public async Task<Resultado<ClienteDetalheDto>> Handle(ObterClientePorIdConsulta request, CancellationToken ct)
        => await clienteServico.ObterPorIdAsync(request.ClienteId, ct);
}
