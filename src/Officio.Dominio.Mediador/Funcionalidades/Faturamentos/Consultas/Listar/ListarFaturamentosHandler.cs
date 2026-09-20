using MediatR;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Faturamentos.Consultas.Listar;

public class ListarFaturamentosHandler(IFaturamentoServico servico)
    : IRequestHandler<ListarFaturamentosConsulta, Resultado<List<FaturamentoMensalDto>>>
{
    public Task<Resultado<List<FaturamentoMensalDto>>> Handle(
        ListarFaturamentosConsulta request,
        CancellationToken cancellationToken)
        => servico.ListarAsync(request.Competencia, request.Status, cancellationToken);
}
