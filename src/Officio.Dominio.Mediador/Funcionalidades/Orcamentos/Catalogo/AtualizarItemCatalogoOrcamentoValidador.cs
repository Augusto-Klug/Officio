using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class AtualizarItemCatalogoOrcamentoValidador : AbstractValidator<AtualizarItemCatalogoOrcamentoComando>
{
    public AtualizarItemCatalogoOrcamentoValidador()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ValorReferencia).GreaterThanOrEqualTo(0).When(x => x.ValorReferencia.HasValue);
    }
}
