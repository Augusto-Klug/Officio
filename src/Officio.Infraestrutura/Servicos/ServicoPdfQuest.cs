using Microsoft.Extensions.Logging;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Servicos;

public class ServicoPdfQuest(ILogger<ServicoPdfQuest> logger) : IServicoPdf
{
    public Task<Stream> GerarContratoAsync(DadosPdfContrato data, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        QuestPDF.Settings.License = LicenseType.Community;
        var stream = new MemoryStream();
        try
        {
            new DocumentoContrato(data).GeneratePdf(stream);
            ct.ThrowIfCancellationRequested();
            stream.Position = 0;
            logger.LogInformation("PDF gerado para contrato {Numero} — {Bytes} bytes", data.NumeroContrato, stream.Length);
            return Task.FromResult<Stream>(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }
}
