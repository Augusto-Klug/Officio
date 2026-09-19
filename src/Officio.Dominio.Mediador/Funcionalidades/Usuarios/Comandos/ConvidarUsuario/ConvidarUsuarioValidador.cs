using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Usuarios.Comandos.ConvidarUsuario;

public class ConvidarUsuarioValidador : AbstractValidator<ConvidarUsuarioComando>
{
    public ConvidarUsuarioValidador()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(450);
        RuleFor(x => x.Perfil).NotEmpty().MaximumLength(50);
    }
}
