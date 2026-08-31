using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public interface IAutentiqueIntegracaoCliente
{
    Task<RespostaCriarDocumentoAutentique?> CriarDocumentoAsync(
        SolicitacaoAssinatura request,
        string apiToken,
        CancellationToken cancellationToken = default);
}
