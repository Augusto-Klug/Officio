using FluentValidation;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.AtualizarSociosProspecto;

public sealed class AtualizarSociosProspectoValidador : AbstractValidator<AtualizarSociosProspectoComando>
{
    public AtualizarSociosProspectoValidador()
    {
        RuleForEach(x => x.Dados.Socios).SetValidator(new SocioRequisicaoValidador());
        RuleFor(x => x.Dados.Socios).Must(s => s is null || s.Sum(x => x.Participacao) <= 100m)
            .WithMessage("A participacao total dos socios nao pode ultrapassar 100%.");
    }
}
