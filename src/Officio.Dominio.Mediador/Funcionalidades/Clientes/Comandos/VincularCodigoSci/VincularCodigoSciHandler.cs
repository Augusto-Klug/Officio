using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Clientes.Comandos.VincularCodigoSci;

public class VincularCodigoSciHandler(IClienteServico clienteServico)
    : IRequestHandler<VincularCodigoSciComando, Resultado<bool>>
{
    public async Task<Resultado<bool>> Handle(VincularCodigoSciComando request, CancellationToken ct)
    {
        var requisicao = new VincularCodigoSciRequisicao(request.ClienteId, request.SciCodigo);

        return await clienteServico.VincularCodigoSciAsync(requisicao, ct);
    }
}
