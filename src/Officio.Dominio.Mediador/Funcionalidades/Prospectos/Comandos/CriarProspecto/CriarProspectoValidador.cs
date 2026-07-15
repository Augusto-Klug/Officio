using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public class CriarProspectoValidador : AbstractValidator<CriarProspectoComando>
{
    public CriarProspectoValidador()
    {
        RuleFor(x => x.RazaoSocial)
            .NotEmpty().WithMessage("RazÃ£o social Ã© obrigatÃ³ria.")
            .MaximumLength(300);

        RuleFor(x => x.Cnpj)
            .Length(18).When(x => !string.IsNullOrEmpty(x.Cnpj))
            .WithMessage("CNPJ invÃ¡lido.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("E-mail invÃ¡lido.");
    }
}





