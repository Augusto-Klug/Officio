using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ConvidarAdministradorParceiro;

public class ConvidarAdministradorParceiroHandler(IGestaoAcessoServico servico)
    : IRequestHandler<ConvidarAdministradorParceiroComando, Resultado<ConviteCriadoDto>>
{
    public Task<Resultado<ConviteCriadoDto>> Handle(
        ConvidarAdministradorParceiroComando request,
        CancellationToken ct)
        => servico.ConvidarAdministradorParceiroAsync(
            request.ParceiroId,
            new ConvidarAdministradorParceiroRequisicao(request.Nome, request.Email),
            ct);
}
