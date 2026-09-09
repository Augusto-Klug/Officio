using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public interface IAutentiqueIntegracaoCliente
{
    Task<RespostaCriarDocumentoAutentique?> CriarDocumentoAsync(
        SolicitacaoAssinatura request,
        string apiToken,
        CancellationToken cancellationToken = default);

    Task<RespostaObterDocumentoAutentique?> ObterDocumentoAsync(
        string documentoId,
        string apiToken,
        CancellationToken cancellationToken = default);

    Task<Stream> BaixarArquivoAsync(
        string url,
        CancellationToken cancellationToken = default);
}
