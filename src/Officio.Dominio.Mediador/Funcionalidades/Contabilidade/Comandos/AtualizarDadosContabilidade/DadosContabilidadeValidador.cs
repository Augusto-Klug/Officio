using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed class DadosContabilidadeValidador : AbstractValidator<DadosContabilidadeDto>
{
    public DadosContabilidadeValidador()
    {
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NomeFantasia).MaximumLength(300);
        RuleFor(x => x.Cnpj).NotEmpty().Must(v => DocumentoFiscal.SomenteDigitos(v)?.Length == 14)
            .WithMessage("CNPJ deve conter 14 dígitos.");
        RuleFor(x => x.Crc).MaximumLength(50);
        RuleFor(x => x.Telefone).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MaximumLength(250);
        RuleFor(x => x.Endereco).SetValidator(new EnderecoContabilidadeValidador()!).When(x => x.Endereco is not null);
        RuleFor(x => x.ResponsavelTecnico).SetValidator(new ResponsavelTecnicoValidador()!).When(x => x.ResponsavelTecnico is not null);
    }
}
