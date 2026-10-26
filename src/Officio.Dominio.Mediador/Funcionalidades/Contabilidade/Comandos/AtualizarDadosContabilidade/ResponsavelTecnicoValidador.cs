using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed class ResponsavelTecnicoValidador : AbstractValidator<ResponsavelTecnicoDto>
{
    public ResponsavelTecnicoValidador()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Cpf).NotEmpty().Must(v => DocumentoFiscal.SomenteDigitos(v)?.Length == 11)
            .WithMessage("CPF deve conter 11 dígitos.");
        RuleFor(x => x.Profissao).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Crc).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Endereco).SetValidator(new EnderecoContabilidadeValidador()!).When(x => x.Endereco is not null);
    }
}
