using FluentValidation;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Mediador.Funcionalidades.Contabilidade.Comandos.AtualizarDadosContabilidade;

public sealed class AtualizarDadosContabilidadeValidador : AbstractValidator<AtualizarDadosContabilidadeComando>
{
    public AtualizarDadosContabilidadeValidador()
    {
        RuleFor(x => x.Dados).NotNull().SetValidator(new DadosContabilidadeValidador());
    }
}
