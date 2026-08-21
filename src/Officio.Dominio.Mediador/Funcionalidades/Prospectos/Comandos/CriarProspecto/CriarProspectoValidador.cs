using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public class CriarProspectoValidador : AbstractValidator<CriarProspectoComando>
{
    public CriarProspectoValidador()
    {
        RuleFor(x => x.Requisicao.RazaoSocial)
            .NotEmpty().WithMessage("RazÃ£o social Ã© obrigatÃ³ria.")
            .MaximumLength(300);

        RuleFor(x => x.Requisicao.Cnpj)
            .Length(18).When(x => !string.IsNullOrEmpty(x.Requisicao.Cnpj))
            .WithMessage("CNPJ invÃ¡lido.");
    }
}





