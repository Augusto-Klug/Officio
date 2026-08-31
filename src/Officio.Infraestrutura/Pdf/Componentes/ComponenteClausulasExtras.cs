using Officio.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

public class ComponenteClausulasExtras(string clausulas) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("6. CLÁUSULAS ADICIONAIS"));

            col.Item().PaddingTop(6)
               .Text(t =>
               {
                   t.DefaultTextStyle(s => s.FontSize(9).LineHeight(1.4f));
                   t.Span(clausulas);
               });
        });
    }

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
}





