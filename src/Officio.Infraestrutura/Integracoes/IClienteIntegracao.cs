namespace Officio.Infraestrutura.Integracoes;

public interface IClienteIntegracao<TConfiguracao>
    where TConfiguracao : class, IConfiguracaoIntegracao
{
    Task<TResposta?> EnviarAsync<TResposta>(
        RequisicaoIntegracao<TResposta> requisicao,
        CancellationToken cancellationToken = default);

    Task<Stream> BaixarAsync(
        string endpoint,
        IReadOnlyDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}
