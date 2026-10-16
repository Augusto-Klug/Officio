using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.ConvidarAdministradorParceiro;

public class ConvidarAdministradorParceiroValidador : AbstractValidator<ConvidarAdministradorParceiroComando>
{
    public ConvidarAdministradorParceiroValidador()
    {
        RuleFor(x => x.ParceiroId).NotEmpty();
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(450);
    }
}
