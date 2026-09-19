using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.AceitarConvite;

public class AceitarConviteValidador : AbstractValidator<AceitarConviteComando>
{
    public AceitarConviteValidador()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Senha).MinimumLength(8).MaximumLength(128);
    }
}
