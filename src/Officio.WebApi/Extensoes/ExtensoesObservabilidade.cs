using System.Diagnostics;
using Officio.WebApi.Servicos.Webhooks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Officio.WebApi.Extensoes;

public static class ExtensoesObservabilidade
{
    public static IServiceCollection AdicionarObservabilidade(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var endpoint = configuration["Observabilidade:Otlp:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
            return services;

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
            throw new InvalidOperationException("Observabilidade:Otlp:Endpoint precisa ser uma URL absoluta.");

        var proporcaoAmostragem = configuration.GetValue("Observabilidade:TraceSampleRatio", 0.1);
        if (proporcaoAmostragem is < 0 or > 1)
            throw new InvalidOperationException("Observabilidade:TraceSampleRatio deve estar entre 0 e 1.");

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Officio.WebApi"))
            .WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(proporcaoAmostragem)))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context =>
                        !context.Request.Path.StartsWithSegments("/openapi") &&
                        !context.Request.Path.StartsWithSegments("/swagger");
                })
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddSource(MetricasWebhookAutentique.ActivitySourceName)
                .AddProcessor(new SanitizadorTagsTelemetria())
                .AddOtlpExporter(options => options.Endpoint = endpointUri))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(MetricasWebhookAutentique.MeterName)
                .AddOtlpExporter(options => options.Endpoint = endpointUri))
            .WithLogging(
                logs => logs.AddOtlpExporter(options => options.Endpoint = endpointUri),
                options =>
                {
                    options.IncludeFormattedMessage = true;
                    options.ParseStateValues = true;
                    options.IncludeScopes = false;
                });

        return services;
    }

    private sealed class SanitizadorTagsTelemetria : BaseProcessor<Activity>
    {
        private static readonly HashSet<string> TagsSensiveis = new(StringComparer.OrdinalIgnoreCase)
        {
            "url.full",
            "url.path",
            "url.query",
            "http.target",
            "db.statement",
            "db.query.text",
            "db.connection_string"
        };

        public override void OnEnd(Activity activity)
        {
            foreach (var tag in activity.TagObjects.ToArray())
            {
                if (TagsSensiveis.Contains(tag.Key))
                    activity.SetTag(tag.Key, null);
            }
        }
    }
}
