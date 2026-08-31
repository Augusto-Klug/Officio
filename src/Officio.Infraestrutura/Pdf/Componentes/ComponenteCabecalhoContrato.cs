using Officio.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

public class ComponenteCabecalhoContrato(
    DadosContratada contratada,
    string         numero,
    DateTime       dataEmissao) : IComponent
{
    public void Compose(IContainer container)
    {
        container
            .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
            .PaddingBottom(10)
            .Row(row =>
            {
                if (contratada.LogoBase64 is not null)
                {
                    var logoBytes = Convert.FromBase64String(contratada.LogoBase64);
                    row.ConstantItem(60)
                       .Image(logoBytes)
                       .FitArea();
                }

                row.RelativeItem()
                   .PaddingLeft(contratada.LogoBase64 is not null ? 12 : 0)
                   .Column(col =>
                   {
                       col.Item()
                          .Text(contratada.RazaoSocial)
                          .Bold().FontSize(13).FontColor(Colors.Black);

                       col.Item()
                          .Text($"CNPJ: {contratada.Cnpj}")
                          .FontSize(9).FontColor(Colors.Grey.Darken2);

                       if (!string.IsNullOrWhiteSpace(contratada.Endereco))
                           col.Item()
                              .Text($"{contratada.Endereco} — {contratada.Cidade}/{contratada.Uf}")
                              .FontSize(9).FontColor(Colors.Grey.Darken2);
                   });

                row.ConstantItem(130)
                   .AlignRight()
                   .Column(col =>
                   {
                       col.Item()
                          .Background(Colors.Blue.Darken3)
                          .Padding(6)
                          .AlignCenter()
                          .Text(numero)
                          .Bold().FontSize(11).FontColor(Colors.White);

                       col.Item()
                          .PaddingTop(4)
                          .AlignRight()
                          .Text($"Emitido em {dataEmissao:dd/MM/yyyy}")
                          .FontSize(8).FontColor(Colors.Grey.Darken1);
                   });
            });
    }
}





