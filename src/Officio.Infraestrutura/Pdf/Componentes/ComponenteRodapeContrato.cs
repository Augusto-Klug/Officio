using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

public class ComponenteRodapeContrato(string numero) : IComponent
{
    public void Compose(IContainer container)
    {
        container
            .BorderTop(1).BorderColor(Colors.Grey.Lighten2)
            .PaddingTop(6)
            .Row(row =>
            {
                row.RelativeItem()
                   .Text(t =>
                   {
                       t.Span(numero)
                        .Bold().FontSize(8)
                        .FontColor(Colors.Grey.Darken2);
                       t.Span(" Â· Documento confidencial.")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Lighten1);
                   });

                row.ConstantItem(120)
                   .AlignRight()
                   .Text(t =>
                   {
                       t.Span("PÃ¡gina ").FontSize(8).FontColor(Colors.Grey.Darken1);
                       t.CurrentPageNumber().Bold().FontSize(8);
                       t.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                       t.TotalPages().Bold().FontSize(8);
                   });
            });
    }
}





