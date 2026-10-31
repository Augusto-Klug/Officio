namespace Officio.Dominio.Interfaces;

public interface IServicoAssinatura
{
    Task<ResultadoBloqueioAssinatura> BloquearDocumentoAsync(string documentoId, CancellationToken ct = default);
    Task<ResultadoAssinatura> EnviarParaAssinaturaAsync(SolicitacaoAssinatura request, CancellationToken ct = default);
    Task<ResultadoEstadoAssinatura> ConsultarEstadoDocumentoAsync(string documentoId, CancellationToken ct = default);
    Task<ResultadoDocumentoAssinado> BaixarDocumentoAssinadoAsync(string documentoId, CancellationToken ct = default);
}

public record ResultadoBloqueioAssinatura(bool Sucesso, string? Mensagem = null);

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

public record LinkAssinante(string Email, string Link, string? Nome = null, string? Tipo = null);

public record ResultadoEstadoAssinatura
{
    public bool Sucesso { get; init; }
    public bool Concluido { get; init; }
    public DateTime? ConcluidoEm { get; init; }
    public IReadOnlyList<string> EmailsSignatarios { get; init; } = [];
    public string? Mensagem { get; init; }

    public static ResultadoEstadoAssinatura CriarSucesso(bool concluido, DateTime? concluidoEm = null, IReadOnlyList<string>? emailsSignatarios = null)
        => new() { Sucesso = true, Concluido = concluido, ConcluidoEm = concluidoEm, EmailsSignatarios = emailsSignatarios ?? [] };

    public static ResultadoEstadoAssinatura CriarFalha(string mensagem)
        => new() { Sucesso = false, Mensagem = mensagem };
}

public record ResultadoDocumentoAssinado
{
    public bool Sucesso { get; init; }
    public Stream? Conteudo { get; init; }
    public string? NomeArquivo { get; init; }
    public string MimeType { get; init; } = "application/pdf";
    public string? Mensagem { get; init; }

    public static ResultadoDocumentoAssinado CriarSucesso(Stream conteudo, string nomeArquivo)
        => new()
        {
            Sucesso = true,
            Conteudo = conteudo,
            NomeArquivo = nomeArquivo
        };

    public static ResultadoDocumentoAssinado CriarFalha(string mensagem)
        => new()
        {
            Sucesso = false,
            Mensagem = mensagem
        };
}





