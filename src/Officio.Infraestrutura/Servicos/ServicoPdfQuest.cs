using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf.Componentes;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Servicos;

public class ServicoPdfQuest(
    ILogger<ServicoPdfQuest> logger) : IServicoPdf
{
    public Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Element(ctx =>
                    new ComponenteCabecalhoContrato(data.Contratada, data.NumeroContrato, data.DataEmissao)
                        .Compose(ctx));

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Element(ctx =>
                        new ComponentePartes(data.Contratada, data.Contratante, data.Socios)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new ComponenteServicos(data.Servicos)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new ComponentePrecificacao(data)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new ComponenteTermos(data)
                            .Compose(ctx));

                    if (!string.IsNullOrWhiteSpace(data.ClausulasExtras))
                        col.Item().Element(ctx =>
                            new ComponenteClausulasExtras(data.ClausulasExtras!)
                                .Compose(ctx));

                    col.Item().Element(ctx =>
                        new ComponenteAssinaturas(data.Contratante, data.Socios, data.DataEmissao)
                            .Compose(ctx));
                });

                page.Footer().Element(ctx =>
                    new ComponenteRodapeContrato(data.NumeroContrato).Compose(ctx));
            });
        });

        var ms = new MemoryStream();
        document.GeneratePdf(ms);
        ms.Position = 0;

        logger.LogInformation(
            "PDF gerado para contrato {Numero} — {Bytes} bytes",
            data.NumeroContrato, ms.Length);

        return Task.FromResult<Stream>(ms);
    }
}





