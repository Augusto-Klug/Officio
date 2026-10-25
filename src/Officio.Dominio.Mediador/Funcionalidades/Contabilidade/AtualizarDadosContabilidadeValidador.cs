using FluentValidation;
using Officio.Dominio.Comum;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade;

public sealed class AtualizarDadosContabilidadeValidador : AbstractValidator<AtualizarDadosContabilidadeComando>
{
    public AtualizarDadosContabilidadeValidador()
    {
        RuleFor(x => x.Dados).NotNull().SetValidator(new DadosContabilidadeValidador());
    }
}

public sealed class DadosContabilidadeValidador : AbstractValidator<DadosContabilidadeDto>
{
    public DadosContabilidadeValidador()
    {
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(300);
        RuleFor(x => x.NomeFantasia).MaximumLength(300);
        RuleFor(x => x.Cnpj).NotEmpty().Must(v => DocumentoFiscal.SomenteDigitos(v)?.Length == 14).WithMessage("CNPJ deve conter 14 dígitos.");
        RuleFor(x => x.Crc).MaximumLength(50);
        RuleFor(x => x.Telefone).MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Email).MaximumLength(250);
        RuleFor(x => x.Endereco).SetValidator(new EnderecoContabilidadeValidador()!).When(x => x.Endereco is not null);
        RuleFor(x => x.ResponsavelTecnico).SetValidator(new ResponsavelTecnicoValidador()!).When(x => x.ResponsavelTecnico is not null);
    }
}

public sealed class ResponsavelTecnicoValidador : AbstractValidator<ResponsavelTecnicoDto>
{
    public ResponsavelTecnicoValidador()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Cpf).NotEmpty().Must(v => DocumentoFiscal.SomenteDigitos(v)?.Length == 11).WithMessage("CPF deve conter 11 dígitos.");
        RuleFor(x => x.Profissao).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Crc).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Endereco).SetValidator(new EnderecoContabilidadeValidador()!).When(x => x.Endereco is not null);
    }
}

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
