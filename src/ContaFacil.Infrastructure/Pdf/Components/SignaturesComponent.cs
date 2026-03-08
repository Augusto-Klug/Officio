using System.Globalization;
using ContaFacil.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFacil.Infrastructure.Pdf.Components;

public class SignaturesComponent(
    ContratanteData          contratante,
    IReadOnlyList<SocioData> socios,
    DateTime                 dataEmissao) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(20)
               .Text(t =>
               {
                   t.Span("Local e Data: ").Bold().FontSize(9);
                   t.Span("___________________________________, ").FontSize(9);
                   t.Span(dataEmissao.ToString("dd 'de' MMMM 'de' yyyy.", new CultureInfo("pt-BR")))
                    .FontSize(9);
               });

            col.Item().PaddingTop(28).Row(row =>
            {
                var adminSocio = socios.FirstOrDefault(s => s.Administrador);

                row.RelativeItem().Element(SignatureBlock(
                    "CONTRATADA",
                    "Contador Responsável",
                    "CRC: _______________"));

                row.ConstantItem(30);

                row.RelativeItem().Element(SignatureBlock(
                    "CONTRATANTE",
                    adminSocio?.Nome ?? contratante.RazaoSocial,
                    adminSocio is not null
                        ? $"CPF: {adminSocio.Cpf}"
                        : $"CNPJ: {contratante.Cnpj}"));
            });

            col.Item().PaddingTop(28)
               .Text("Testemunhas:").Bold().FontSize(9)
               .FontColor(Colors.Grey.Darken1);

            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Element(WitnessBlock("1ª Testemunha"));
                row.ConstantItem(30);
                row.RelativeItem().Element(WitnessBlock("2ª Testemunha"));
            });
        });
    }

    static Action<IContainer> SignatureBlock(
        string label, string nome, string doc) =>
        c => c.Column(col =>
        {
            col.Item()
               .BorderBottom(1).BorderColor(Colors.Black)
               .Height(40);

            col.Item().PaddingTop(4)
               .AlignCenter()
               .Text(nome).Bold().FontSize(9);

            col.Item().AlignCenter()
               .Text(label).FontSize(8)
               .FontColor(Colors.Grey.Darken2);

            col.Item().AlignCenter()
               .Text(doc).FontSize(8)
               .FontColor(Colors.Grey.Darken2);
        });

    static Action<IContainer> WitnessBlock(string label) =>
        c => c.Column(col =>
        {
            col.Item()
               .BorderBottom(1).BorderColor(Colors.Black)
               .Height(30);

            col.Item().PaddingTop(3).Row(row =>
            {
                row.RelativeItem()
                   .Text($"{label}:").FontSize(8).Bold();
                row.RelativeItem()
                   .Text("Nome: ___________________________").FontSize(8);
            });

            col.Item().PaddingTop(2)
               .Text("CPF: _______________________________")
               .FontSize(8);
        });
}
