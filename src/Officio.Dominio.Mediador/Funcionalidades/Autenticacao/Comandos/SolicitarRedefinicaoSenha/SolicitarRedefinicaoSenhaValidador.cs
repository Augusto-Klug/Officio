using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Autenticacao.Comandos.SolicitarRedefinicaoSenha;

public class SolicitarRedefinicaoSenhaValidador : AbstractValidator<SolicitarRedefinicaoSenhaComando>
{
    public SolicitarRedefinicaoSenhaValidador()
        => RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(450);
}
