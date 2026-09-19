using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.CriarEmpresaAdministrador;

public class CriarEmpresaAdministradorHandler(IGestaoAcessoServico servico)
    : IRequestHandler<CriarEmpresaAdministradorComando, Resultado<EmpresaOnboardingDto>>
{
    public Task<Resultado<EmpresaOnboardingDto>> Handle(CriarEmpresaAdministradorComando request, CancellationToken ct)
        => servico.CriarEmpresaAdministradorAsync(new(
            request.Cnpj,
            request.RazaoSocial,
            request.NomeFantasia,
            request.EmailEmpresa,
            request.NomeAdministrador,
            request.EmailAdministrador), ct);
}
