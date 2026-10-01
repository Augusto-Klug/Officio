using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Officio.WebApi.Servicos.Webhooks;

internal static class MetricasWebhookAutentique
{
    public const string MeterName = "Officio.WebApi.Webhooks";
    public const string ActivitySourceName = "Officio.WebApi.Webhooks";

    private static readonly Meter Meter = new(MeterName);
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    private static readonly Counter<long> Recebidos = Meter.CreateCounter<long>("officio.webhook.received");
    private static readonly Counter<long> Enfileirados = Meter.CreateCounter<long>("officio.webhook.enqueued");
    private static readonly Counter<long> Rejeitados = Meter.CreateCounter<long>("officio.webhook.rejected");
    private static readonly Counter<long> Duplicados = Meter.CreateCounter<long>("officio.webhook.duplicates");
    private static readonly Counter<long> FalhasProcessamento = Meter.CreateCounter<long>("officio.webhook.processing.failures");
    private static readonly Histogram<double> DuracaoProcessamento = Meter.CreateHistogram<double>(
        "officio.webhook.processing.duration",
        "ms");

    public static Activity? IniciarProcessamento(ActivityContext contextoPai)
        => ActivitySource.StartActivity(
            "autentique.webhook.process",
            ActivityKind.Consumer,
            contextoPai);

    public static void RegistrarRecebido(string resultado)
    {
        Recebidos.Add(1, new KeyValuePair<string, object?>("result", resultado));
        if (resultado == "accepted")
            Enfileirados.Add(1);
        else if (resultado == "duplicate")
            Duplicados.Add(1);
        else
            Rejeitados.Add(1, new KeyValuePair<string, object?>("reason", resultado));
    }

    public static void RegistrarProcessamento(bool sucesso, double duracaoMs)
    {
        DuracaoProcessamento.Record(
            duracaoMs,
            new KeyValuePair<string, object?>("result", sucesso ? "success" : "failure"));

        if (!sucesso)
            FalhasProcessamento.Add(1);
    }

}
