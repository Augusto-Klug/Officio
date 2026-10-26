using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

public sealed class EnderecoSocioValidador : AbstractValidator<EnderecoSocioRequisicao>
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
