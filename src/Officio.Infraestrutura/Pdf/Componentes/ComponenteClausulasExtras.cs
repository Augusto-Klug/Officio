using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteClausulasExtras(string clausulas) : IComponent
{
    public void Compose(IContainer container) => container.Column(col =>
    {
        col.Spacing(6);
        col.Item().EnsureSpace(50).Text("CLÁUSULAS ADICIONAIS").Bold();
        col.Item().Text(clausulas).Justify();
    });
}
