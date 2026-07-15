using Officio.Dominio.Servicos.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

public class ComponentePartes(
    DadosContratada           contratada,
    DadosContratante          contratante,
    IReadOnlyList<DadosSocio> socios) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("1. DAS PARTES"));

            col.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Element(PartyBox(
                    "CONTRATADA",
                    Colors.Blue.Darken3,
                    [
                        ("RazÃ£o Social", contratada.RazaoSocial),
                        ("CNPJ",         contratada.Cnpj),
                        ("EndereÃ§o",     $"{contratada.Endereco}, {contratada.Cidade}/{contratada.Uf}"),
                        ("E-mail",       contratada.Email),
                        ("Telefone",     contratada.Telefone),
                    ]));

                row.ConstantItem(12);

                row.RelativeItem().Element(PartyBox(
                    "CONTRATANTE",
                    Colors.Green.Darken3,
                    [
                        ("RazÃ£o Social",       contratante.RazaoSocial),
                        ("Nome Fantasia",      contratante.NomeFantasia),
                        ("CNPJ",              contratante.Cnpj),
                        ("Regime TributÃ¡rio", contratante.RegimeTributario),
                        ("EndereÃ§o",          $"{contratante.Endereco}, {contratante.Cidade}/{contratante.Uf}"),
                        ("E-mail",            contratante.Email),
                    ]));
            });

            if (socios.Count > 0)
            {
                col.Item().PaddingTop(10)
                   .Text("SÃ³cios e ResponsÃ¡veis").Bold().FontSize(9);

                col.Item().PaddingTop(4).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        foreach (var h in new[] { "Nome", "CPF", "ParticipaÃ§Ã£o", "Administrador" })
                            header.Cell()
                                  .Background(Colors.Grey.Lighten3)
                                  .Padding(4)
                                  .Text(h).Bold().FontSize(8);
                    });

                    foreach (var (s, i) in socios.Select((s, i) => (s, i)))
                    {
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        table.Cell().Background(bg).Padding(4).Text(s.Nome).FontSize(9);
                        table.Cell().Background(bg).Padding(4).Text(s.Cpf).FontSize(9);
                        table.Cell().Background(bg).Padding(4).AlignCenter()
                             .Text($"{s.Participacao:F0}%").FontSize(9);
                        table.Cell().Background(bg).Padding(4).AlignCenter()
                             .Text(s.Administrador ? "Sim" : "â€”").FontSize(9);
                    }
                });
            }
        });
    }

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);

    static Action<IContainer> PartyBox(
        string label, string headerColor,
        IEnumerable<(string Key, string Valor)> fields) =>
        c => c.Border(1).BorderColor(Colors.Grey.Lighten2).Column(col =>
        {
            col.Item().Background(headerColor).Padding(5)
               .Text(label).Bold().FontSize(9).FontColor(Colors.White);

            foreach (var (key, value) in fields)
                col.Item()
                   .BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                   .Padding(4)
                   .Row(r =>
                   {
                       r.ConstantItem(90)
                        .Text(key + ":").FontSize(8).FontColor(Colors.Grey.Darken2);
                       r.RelativeItem()
                        .Text(value).FontSize(9).Bold();
                   });
        });
}





