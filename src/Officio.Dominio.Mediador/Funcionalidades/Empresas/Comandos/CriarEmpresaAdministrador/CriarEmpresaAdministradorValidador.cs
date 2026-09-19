using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Empresas.Comandos.CriarEmpresaAdministrador;

public class CriarEmpresaAdministradorValidador : AbstractValidator<CriarEmpresaAdministradorComando>
{
    public CriarEmpresaAdministradorValidador()
    {
        RuleFor(x => x.Cnpj).Must(cnpj => cnpj.Count(char.IsDigit) == 14).WithMessage("CNPJ deve possuir 14 digitos.");
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NomeFantasia).MaximumLength(300);
        RuleFor(x => x.EmailEmpresa).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.EmailEmpresa));
        RuleFor(x => x.NomeAdministrador).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EmailAdministrador).NotEmpty().EmailAddress().MaximumLength(450);
    }
}
