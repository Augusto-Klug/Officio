using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class CriarItemCatalogoOrcamentoValidador : AbstractValidator<CriarItemCatalogoOrcamentoComando>
{
    public CriarItemCatalogoOrcamentoValidador()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ValorReferencia).GreaterThanOrEqualTo(0).When(x => x.ValorReferencia.HasValue);
    }
}
