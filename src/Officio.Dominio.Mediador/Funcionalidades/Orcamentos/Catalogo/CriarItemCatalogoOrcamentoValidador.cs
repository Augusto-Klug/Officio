using FluentValidation;

namespace Officio.Dominio.Mediador.Funcionalidades.Orcamentos.Catalogo;

public class CriarItemCatalogoOrcamentoValidador : AbstractValidator<CriarItemCatalogoOrcamentoComando>
{
    public CriarItemCatalogoOrcamentoValidador()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Secao).IsInEnum();
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.NaturezaCobranca).IsInEnum();
        RuleFor(x => x.ValorReferencia).NotNull().When(x => x.Tipo == Officio.Dominio.Enumeradores.TipoItemOrcamento.ItemMonetario);
        RuleFor(x => x.ValorReferencia).GreaterThanOrEqualTo(0).When(x => x.ValorReferencia.HasValue);
    }
}
