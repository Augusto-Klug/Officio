using ContaFacil.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFacil.Infrastructure.Pdf.Components;

public class PricingComponent(ContractPdfData data) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("3. DA REMUNERAÇÃO"));

            col.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1.5f);
                    c.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    foreach (var h in new[] { "Descrição", "Qtd.", "Valor Unit.", "Total" })
                        header.Cell()
                              .Background("#1e3a5f")
                              .Padding(6)
                              .AlignCenter()
                              .Text(h).Bold().FontSize(9).FontColor(Colors.White);
                });

                AddRow(table, "Honorários Mensais (base)", 1,
                    data.ValorMensalidade, data.ValorMensalidade, 0);

                if (data.ValorPorFuncionario > 0 && data.QtdFuncionarios > 0)
                    AddRow(table, "Taxa por Funcionário", data.QtdFuncionarios,
                        data.ValorPorFuncionario,
                        data.ValorPorFuncionario * data.QtdFuncionarios, 1);

                if (data.ValorTaxasExtras > 0)
                    AddRow(table, "Taxas e Serviços Adicionais", 1,
                        data.ValorTaxasExtras, data.ValorTaxasExtras, 2);

                if (data.Desconto > 0)
                    AddRow(table, "Desconto Concedido", 1,
                        -data.Desconto, -data.Desconto, 3,
                        Colors.Red.Lighten4);

                // Total row
                table.Cell().ColumnSpan(2)
                     .Background(Colors.Grey.Lighten3)
                     .Padding(6)
                     .Text("TOTAL MENSAL").Bold().FontSize(10);

                table.Cell()
                     .Background(Colors.Grey.Lighten3)
                     .Padding(6);

                table.Cell()
                     .Background("#1e3a5f").Padding(6)
                     .AlignRight()
                     .Text($"R$ {data.ValorTotal:N2}")
                     .Bold().FontSize(11).FontColor(Colors.White);
            });

            col.Item().PaddingTop(6)
               .Text(t =>
               {
                   t.Span("Vencimento:").Bold().FontSize(9);
                   t.Span($" Todo dia {data.DiaVencimento} de cada mês.").FontSize(9);
               });
        });
    }

    static void AddRow(
        TableDescriptor table, string desc,
        int qtd, decimal unit, decimal total,
        int rowIndex, string? bgOverride = null)
    {
        var bg = bgOverride ?? (rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten5);

        table.Cell().Background(bg).Padding(5).Text(desc).FontSize(9);
        table.Cell().Background(bg).Padding(5).AlignCenter()
             .Text(qtd.ToString()).FontSize(9);
        table.Cell().Background(bg).Padding(5).AlignRight()
             .Text($"R$ {unit:N2}").FontSize(9);
        table.Cell().Background(bg).Padding(5).AlignRight()
             .Text($"R$ {total:N2}").Bold().FontSize(9);
    }

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
}
