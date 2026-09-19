using MediatR;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.AlterarStatusEmpresa;

public class AlterarStatusEmpresaHandler(IGestaoAcessoServico servico)
    : IRequestHandler<AlterarStatusEmpresaComando, Resultado<bool>>
{
    public Task<Resultado<bool>> Handle(AlterarStatusEmpresaComando request, CancellationToken ct)
        => servico.AlterarStatusEmpresaAsync(request.EmpresaId, request.Ativa, ct);
}
