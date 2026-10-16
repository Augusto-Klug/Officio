using FluentValidation;
using Officio.Dominio.Mediador.Funcionalidades.Prospectos.Validadores;

namespace Officio.Dominio.Mediador.Funcionalidades.Prospectos.Comandos.CriarProspecto;

public class CriarProspectoValidador : AbstractValidator<CriarProspectoComando>
{
    public CriarProspectoValidador()
    {
        RuleFor(x => x.Requisicao).SetValidator(new ProspectoRequisicaoValidador());
    }
}





