using Officio.Infraestrutura.Integracoes;

namespace Officio.Infraestrutura.Integracoes.Autentique;

public class OpcoesAutentique : IConfiguracaoIntegracao
{
    public string BaseUrl { get; set; } = "https://api.autentique.com.br/v2/graphql";
    public string ApiToken { get; set; } = "";
    public string WebhookSecret { get; set; } = "";
    public string? OrganizationId { get; set; }
    public string? FolderId { get; set; }
}
