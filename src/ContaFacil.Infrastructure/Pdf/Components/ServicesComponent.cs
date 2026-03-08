using ContaFacil.Application.Common.Models.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFacil.Infrastructure.Pdf.Components;

public class ServicesComponent(IReadOnlyList<ServicoData> servicos) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("2. DOS SERVIÇOS"));

            if (servicos.Count == 0)
            {
                col.Item().PaddingTop(6)
                   .Text("Serviços contábeis conforme acordado entre as partes.")
                   .FontSize(9);
                return;
            }

            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Descrição", "Periodicidade", "Valor" })
                        header.Cell()
                              .Background(Colors.Grey.Lighten3)
                              .Padding(4)
                              .Text(h).Bold().FontSize(8);
                });

                foreach (var (s, i) in servicos.Select((s, i) => (s, i)))
                {
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                    table.Cell().Background(bg).Padding(4).Text(s.Descricao).FontSize(9);
                    table.Cell().Background(bg).Padding(4).AlignCenter()
                         .Text(s.Periodicidade).FontSize(9);
                    table.Cell().Background(bg).Padding(4).AlignRight()
                         .Text($"R$ {s.Valor:N2}").FontSize(9);
                }
            });
        });
    }

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
}
