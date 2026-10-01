using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Officio.Infra.BancoDeDados.Entidades;

public enum StatusWebhookAutentique
{
    Pendente = 0,
    Processando = 1,
    Processado = 2,
    Falha = 3
}

public sealed class WebhookAutentiqueEvento
{
    private WebhookAutentiqueEvento() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string HashPayload { get; private set; } = default!;
    public string? Payload { get; private set; }
    public string? TraceParent { get; private set; }
    public string? TraceState { get; private set; }
    public DateTime RecebidoEm { get; private set; }
    public DateTime? ProximaTentativaEm { get; private set; }
    public DateTime? BloqueadoAte { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public StatusWebhookAutentique Status { get; private set; }
    public string? UltimoErro { get; private set; }

    public static WebhookAutentiqueEvento Criar(string payload, ActivityContext contextoTrace)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var traceId = contextoTrace.TraceId.ToHexString();
        var spanId = contextoTrace.SpanId.ToHexString();
        var traceParent = traceId.All(caractere => caractere == '0') ||
                          spanId.All(caractere => caractere == '0')
            ? null
            : $"00-{traceId}-{spanId}-{(byte)contextoTrace.TraceFlags:x2}";
        return new WebhookAutentiqueEvento
        {
            HashPayload = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
            Payload = payload,
            TraceParent = traceParent,
            TraceState = contextoTrace.TraceState,
            RecebidoEm = DateTime.UtcNow,
            ProximaTentativaEm = DateTime.UtcNow,
            Status = StatusWebhookAutentique.Pendente
        };
    }
}
