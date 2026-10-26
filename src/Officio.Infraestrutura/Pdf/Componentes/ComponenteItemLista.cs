using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteItemLista(string texto) : IComponent
{
    public void Compose(IContainer container) => container.Row(row =>
    {
        row.ConstantItem(12).Text("•");
        row.RelativeItem().Text(texto).Justify();
    });
}
