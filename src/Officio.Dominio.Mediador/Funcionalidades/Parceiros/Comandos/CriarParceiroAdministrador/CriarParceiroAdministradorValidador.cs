using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Parceiros.Comandos.CriarParceiroAdministrador;

public class CriarParceiroAdministradorValidador : AbstractValidator<CriarParceiroAdministradorComando>
{
    public CriarParceiroAdministradorValidador()
    {
        RuleFor(x => x.Cnpj).Must(cnpj => cnpj.Count(char.IsDigit) == 14).WithMessage("CNPJ deve possuir 14 digitos.");
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NomeFantasia).MaximumLength(300);
        RuleFor(x => x.EmailParceiro).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailParceiro));
        RuleFor(x => x.NomeAdministrador).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmailAdministrador).NotEmpty().EmailAddress().MaximumLength(450);
    }
}
