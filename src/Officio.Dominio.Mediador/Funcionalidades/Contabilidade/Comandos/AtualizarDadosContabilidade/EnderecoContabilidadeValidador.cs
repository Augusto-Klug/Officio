using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed class EnderecoContabilidadeValidador : AbstractValidator<EnderecoContabilidadeDto>
{
    public EnderecoContabilidadeValidador()
    {
        RuleFor(x => x.Cep).Must(v => DocumentoFiscal.SomenteDigitos(v)?.Length == 8)
            .When(x => !string.IsNullOrWhiteSpace(x.Cep)).WithMessage("CEP deve conter 8 dígitos.");
        RuleFor(x => x.Logradouro).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Numero).MaximumLength(30);
        RuleFor(x => x.Complemento).MaximumLength(150);
        RuleFor(x => x.Bairro).MaximumLength(150);
        RuleFor(x => x.Cidade).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Uf).NotEmpty().Matches("^[A-Za-z]{2}$");
        RuleFor(x => x.MunicipioCodigoIbge).NotEmpty().Matches("^[0-9]{7}$");
    }
}
