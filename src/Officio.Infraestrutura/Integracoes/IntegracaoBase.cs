namespace Officio.Infraestrutura.Integracoes;

public abstract class IntegracaoBase<TConfiguracao>(
    IClienteIntegracao<TConfiguracao> clienteIntegracao)
    where TConfiguracao : class, IConfiguracaoIntegracao
{
    protected IClienteIntegracao<TConfiguracao> ClienteIntegracao { get; } = clienteIntegracao;
}
