namespace Officio.Dominio.Servicos.Interfaces;

public interface IServicoAssinatura
{
    Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default);
    Task<ResultadoStatusAssinatura> ObterStatusAsync(string envelopeId, CancellationToken ct = default);
}

public record SolicitacaoAssinatura(
    Stream FluxoPdf,
    string NomeDocumento,
    List<DadosAssinante> Assinantes);

public record DadosAssinante(string Nome, string Email, string? Cpf, string Tipo);

public record ResultadoAssinatura
{
    public bool Sucesso { get; init; }
    public string? EnvelopeId { get; init; }
    public List<LinkAssinante> LinksAssinantes { get; init; } = [];
    public bool RequerFallback { get; init; }
    public string? Mensagem { get; init; }

    public static ResultadoAssinatura CriarSucesso(string envelopeId, List<LinkAssinante> links)
        => new() { Sucesso = true, EnvelopeId = envelopeId, LinksAssinantes = links };

    public static ResultadoAssinatura CriarFalha(string mensagem)
        => new() { Sucesso = false, RequerFallback = true, Mensagem = mensagem };
}

public record LinkAssinante(string Email, string Link);

public record ResultadoStatusAssinatura(string Status, DateTime? AssinadoEm);





