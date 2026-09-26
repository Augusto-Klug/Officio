using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Consultas.ListarParceiros;

public class ListarParceirosHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ListarParceirosConsulta, Resultado<List<ParceiroPlataformaDto>>>
{
    public Task<Resultado<List<ParceiroPlataformaDto>>> Handle(
        ListarParceirosConsulta request,
        CancellationToken ct) => servico.ListarParceirosAsync(ct);
}
