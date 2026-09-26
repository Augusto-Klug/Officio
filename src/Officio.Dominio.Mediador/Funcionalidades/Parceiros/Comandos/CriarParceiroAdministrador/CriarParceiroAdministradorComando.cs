using MediatR;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.CriarParceiroAdministrador;

public record CriarParceiroAdministradorComando(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string? EmailParceiro,
    string NomeAdministrador,
    string EmailAdministrador) : IRequest<Resultado<ParceiroOnboardingDto>>;
