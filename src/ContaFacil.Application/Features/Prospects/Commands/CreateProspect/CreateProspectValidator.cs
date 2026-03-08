using FluentValidation;

namespace ContaFacil.Application.Features.Prospects.Commands.CreateProspect;

public class CreateProspectValidator : AbstractValidator<CreateProspectCommand>
{
    public CreateProspectValidator()
    {
        RuleFor(x => x.RazaoSocial)
            .NotEmpty().WithMessage("Razão social é obrigatória.")
            .MaximumLength(300);

        RuleFor(x => x.Cnpj)
            .Length(18).When(x => !string.IsNullOrEmpty(x.Cnpj))
            .WithMessage("CNPJ inválido.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("E-mail inválido.");
    }
}
