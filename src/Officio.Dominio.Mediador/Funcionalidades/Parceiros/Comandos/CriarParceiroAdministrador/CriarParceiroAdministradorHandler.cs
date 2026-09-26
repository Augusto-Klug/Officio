using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.CriarParceiroAdministrador;

public class CriarParceiroAdministradorHandler(IGestaoAcessoServico servico)
    : IRequestHandler<CriarParceiroAdministradorComando, Resultado<ParceiroOnboardingDto>>
{
    public Task<Resultado<ParceiroOnboardingDto>> Handle(CriarParceiroAdministradorComando request, CancellationToken ct)
        => servico.CriarParceiroAdministradorAsync(new(
            request.Cnpj,
            request.RazaoSocial,
            request.NomeFantasia,
            request.EmailParceiro,
            request.NomeAdministrador,
            request.EmailAdministrador), ct);
}
