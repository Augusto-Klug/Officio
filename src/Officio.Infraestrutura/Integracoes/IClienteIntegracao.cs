namespace Officio.Infraestrutura.Integracoes;

public interface IClienteIntegracao<TConfiguracao>
    where TConfiguracao : class, IConfiguracaoIntegracao
{
    Task<TResposta?> EnviarAsync<TResposta>(
        RequisicaoIntegracao<TResposta> requisicao,
        CancellationToken cancellationToken = default);
}
