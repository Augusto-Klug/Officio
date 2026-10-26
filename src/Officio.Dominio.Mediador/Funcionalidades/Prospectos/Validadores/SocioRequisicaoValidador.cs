using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

public sealed class SocioRequisicaoValidador : AbstractValidator<CriarSocioRequisicao>
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
