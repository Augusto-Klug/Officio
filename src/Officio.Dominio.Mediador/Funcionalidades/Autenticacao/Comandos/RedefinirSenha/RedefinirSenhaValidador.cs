using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.RedefinirSenha;

public class RedefinirSenhaValidador : AbstractValidator<RedefinirSenhaComando>
{
    public RedefinirSenhaValidador()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.Senha).MinimumLength(8).MaximumLength(128);
    }
}
