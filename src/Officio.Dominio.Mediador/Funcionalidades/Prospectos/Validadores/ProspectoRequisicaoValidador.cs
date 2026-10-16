using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

public class ProspectoRequisicaoValidador : AbstractValidator<CriarProspectoRequisicao>
{
    public ProspectoRequisicaoValidador()
    {
        RuleFor(x => x.RazaoSocial).NotEmpty().WithMessage("Raz\u00e3o social \u00e9 obrigat\u00f3ria.").MaximumLength(300);
        RuleFor(x => x.Cnpj)
            .Must(cnpj => DocumentoFiscal.SomenteDigitos(cnpj)?.Length == 14)
            .When(x => !string.IsNullOrWhiteSpace(x.Cnpj)).WithMessage("CNPJ inv\u00e1lido.");
        RuleFor(x => x.Cep)
            .Must(cep => DocumentoFiscal.SomenteDigitos(cep)?.Length == 8)
            .When(x => !string.IsNullOrWhiteSpace(x.Cep)).WithMessage("CEP invalido.");
        RuleFor(x => x.MunicipioCodigoIbge).Matches("^[0-9]{7}$")
            .When(x => !string.IsNullOrWhiteSpace(x.MunicipioCodigoIbge))
            .WithMessage("Codigo IBGE do municipio invalido.");
        RuleFor(x => x.RegimeTributario).IsInEnum();
        RuleForEach(x => x.Socios).ChildRules(socio =>
        {
            socio.RuleFor(x => x.Nome).NotEmpty().MaximumLength(250);
            socio.RuleFor(x => x.Cpf).Must(cpf => DocumentoFiscal.SomenteDigitos(cpf)?.Length == 11)
                .When(x => !string.IsNullOrWhiteSpace(x.Cpf)).WithMessage("CPF do socio invalido.");
            socio.RuleFor(x => x.Participacao).InclusiveBetween(0m, 100m);
        });
        RuleFor(x => x.Socios).Must(socios => socios is null || socios.Sum(x => x.Participacao) <= 100m)
            .WithMessage("A participacao total dos socios nao pode ultrapassar 100%.");
    }
}
