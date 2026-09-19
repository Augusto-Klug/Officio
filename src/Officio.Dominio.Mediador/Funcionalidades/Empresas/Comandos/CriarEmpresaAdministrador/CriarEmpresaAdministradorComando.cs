using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.CriarEmpresaAdministrador;

public record CriarEmpresaAdministradorComando(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string? EmailEmpresa,
    string NomeAdministrador,
    string EmailAdministrador) : IRequest<Resultado<EmpresaOnboardingDto>>;
