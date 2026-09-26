using FluentValidation;
using Officio.Dominio.Comum;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public class CriarProspectoValidador : AbstractValidator<CriarProspectoComando>
{
    public CriarProspectoValidador()
    {
        RuleFor(x => x.Requisicao.RazaoSocial)
            .NotEmpty().WithMessage("Razão social é obrigatória.")
            .MaximumLength(300);

        RuleFor(x => x.Requisicao.Cnpj)
            .Must(cnpj => DocumentoFiscal.SomenteDigitos(cnpj)?.Length == 14)
            .When(x => !string.IsNullOrWhiteSpace(x.Requisicao.Cnpj))
            .WithMessage("CNPJ inválido.");
        RuleFor(x => x.Requisicao.Cep)
            .Must(cep => DocumentoFiscal.SomenteDigitos(cep)?.Length == 8)
            .When(x => !string.IsNullOrWhiteSpace(x.Requisicao.Cep))
            .WithMessage("CEP invalido.");

        RuleFor(x => x.Requisicao.MunicipioCodigoIbge)
            .Matches("^[0-9]{7}$")
            .When(x => !string.IsNullOrWhiteSpace(x.Requisicao.MunicipioCodigoIbge))
            .WithMessage("Codigo IBGE do municipio invalido.");
    }
}





