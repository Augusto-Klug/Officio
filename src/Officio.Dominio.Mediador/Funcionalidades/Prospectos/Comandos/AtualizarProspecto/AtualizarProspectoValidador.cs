using FluentValidation;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarProspecto;

public class AtualizarProspectoValidador : AbstractValidator<AtualizarProspectoComando>
{
    public AtualizarProspectoValidador()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Requisicao).SetValidator(new ProspectoRequisicaoValidador());
    }
}
