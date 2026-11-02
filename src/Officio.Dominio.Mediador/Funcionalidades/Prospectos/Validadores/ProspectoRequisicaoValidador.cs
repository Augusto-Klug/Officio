using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

public sealed class ProspectoRequisicaoValidador : AbstractValidator<CriarProspectoRequisicao>
{
    public ProspectoRequisicaoValidador()
    {
        RuleFor(x => x.RazaoSocial).NotEmpty().WithMessage("Raz\u00e3o social \u00e9 obrigat\u00f3ria.").MaximumLength(300);
        RuleFor(x => x.NomeContato).NotEmpty().WithMessage("Nome do contato \u00e9 obrigat\u00f3rio.").MaximumLength(250);
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("E-mail \u00e9 obrigat\u00f3rio.").EmailAddress().MaximumLength(250);
        RuleFor(x => x.Telefone)
            .Must(telefone => DocumentoFiscal.SomenteDigitos(telefone)?.Length is 10 or 11)
            .WithMessage("Informe um telefone com DDD e 10 ou 11 d\u00edgitos.");
        RuleFor(x => x.Cnpj)
            .Must(cnpj => DocumentoFiscal.SomenteDigitos(cnpj)?.Length == 14)
            .When(x => !string.IsNullOrWhiteSpace(x.Cnpj)).WithMessage("CNPJ inv\u00e1lido.");
        RuleFor(x => x.Numero).NotEmpty().WithMessage("N\u00famero do endere\u00e7o \u00e9 obrigat\u00f3rio.").MaximumLength(30);
        RuleFor(x => x.Cep).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("CEP \u00e9 obrigat\u00f3rio.")
            .Must(cep => DocumentoFiscal.SomenteDigitos(cep)?.Length == 8).WithMessage("CEP inv\u00e1lido.");
        RuleFor(x => x.Logradouro).NotEmpty().WithMessage("Logradouro \u00e9 obrigat\u00f3rio.").MaximumLength(250);
        RuleFor(x => x.Bairro).NotEmpty().WithMessage("Bairro \u00e9 obrigat\u00f3rio.").MaximumLength(150);
        RuleFor(x => x.Uf).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("UF \u00e9 obrigat\u00f3ria.").Matches("^[a-zA-Z]{2}$");
        RuleFor(x => x.Cidade).NotEmpty().WithMessage("Munic\u00edpio \u00e9 obrigat\u00f3rio.").MaximumLength(150);
        RuleFor(x => x.MunicipioCodigoIbge).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Munic\u00edpio \u00e9 obrigat\u00f3rio.").Matches("^[0-9]{7}$")
            .WithMessage("C\u00f3digo IBGE do munic\u00edpio inv\u00e1lido.");
        RuleFor(x => x.RegimeTributario).IsInEnum();
        RuleForEach(x => x.Socios).SetValidator(new SocioRequisicaoValidador());
        RuleFor(x => x.Socios).Must(socios => socios is null || socios.Sum(x => x.Participacao) <= 100m)
            .WithMessage("A participacao total dos socios nao pode ultrapassar 100%.");
    }
}
