using ContaFacil.Application.Common.Interfaces;
using ContaFacil.Application.Common.Models.Pdf;
using ContaFacil.Infrastructure.Pdf.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ContaFacil.Infrastructure.Services;

public class QuestPdfService(
    IOptions<PdfOptions> options,
    ILogger<QuestPdfService> logger) : IPdfService
{
    public Task<Stream> GenerateContractAsync(ContractPdfData data, CancellationToken ct = default)
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
                    new ContractHeaderComponent(data.Contratada, data.NumeroContrato, data.DataEmissao)
                        .Compose(ctx));

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Element(ctx =>
                        new PartiesComponent(data.Contratada, data.Contratante, data.Socios)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new ServicesComponent(data.Servicos)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new PricingComponent(data)
                            .Compose(ctx));

                    col.Item().Element(ctx =>
                        new TermsComponent(data)
                            .Compose(ctx));

                    if (!string.IsNullOrWhiteSpace(data.ClausulasExtras))
                        col.Item().Element(ctx =>
                            new ExtraClausesComponent(data.ClausulasExtras!)
                                .Compose(ctx));

                    col.Item().Element(ctx =>
                        new SignaturesComponent(data.Contratante, data.Socios, data.DataEmissao)
                            .Compose(ctx));
                });

                page.Footer().Element(ctx =>
                    new ContractFooterComponent(data.NumeroContrato).Compose(ctx));
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
