using Officio.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Servicos;

public sealed class ConversorImagensPdfQuest : IConversorImagensPdf
{
    public byte[] CriarPdf(IReadOnlyList<byte[]> imagens)
    {
        if (imagens.Count == 0)
            throw new ArgumentException("Informe ao menos uma imagem.", nameof(imagens));

        QuestPDF.Settings.License = LicenseType.Community;
        var documento = Document.Create(container =>
        {
            foreach (var imagem in imagens)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);
                    page.Content().Image(imagem).FitArea();
                });
            }
        });

        return documento.GeneratePdf();
    }
}
