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
        RuleForEach(x => x.Socios).SetValidator(new SocioRequisicaoValidador());
        RuleFor(x => x.Socios).Must(socios => socios is null || socios.Sum(x => x.Participacao) <= 100m)
            .WithMessage("A participacao total dos socios nao pode ultrapassar 100%.");
    }
}

public class SocioRequisicaoValidador : AbstractValidator<CriarSocioRequisicao>
{
    public SocioRequisicaoValidador()
    {
        RuleFor(x => x.Nome).MaximumLength(250);
        RuleFor(x => x.Profissao).MaximumLength(250);
        RuleFor(x => x.Cpf).Must(cpf => DocumentoFiscal.SomenteDigitos(cpf)?.Length == 11)
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf)).WithMessage("CPF do socio invalido.");
        RuleFor(x => x.Participacao).InclusiveBetween(0m, 100m);
        When(x => x.Endereco is not null, () => RuleFor(x => x.Endereco!).SetValidator(new EnderecoSocioValidador()));
    }
}

public class EnderecoSocioValidador : AbstractValidator<EnderecoSocioRequisicao>
{
    public EnderecoSocioValidador()
    {
        RuleFor(x => x.Cep).Must(c => DocumentoFiscal.SomenteDigitos(c)?.Length == 8).When(x => !string.IsNullOrWhiteSpace(x.Cep));
        RuleFor(x => x.MunicipioCodigoIbge).Matches("^[0-9]{7}$").When(x => !string.IsNullOrWhiteSpace(x.MunicipioCodigoIbge));
        RuleFor(x => x.Uf).Matches("^[a-zA-Z]{2}$").When(x => !string.IsNullOrWhiteSpace(x.Uf));
        RuleFor(x => x.Logradouro).MaximumLength(250);
        RuleFor(x => x.Numero).MaximumLength(30);
        RuleFor(x => x.Complemento).MaximumLength(150);
        RuleFor(x => x.Bairro).MaximumLength(150);
    }
}
