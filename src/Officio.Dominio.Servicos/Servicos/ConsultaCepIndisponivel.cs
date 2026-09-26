using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class ConsultaCepIndisponivel : IConsultaCep
{
    public Task<EnderecoCep?> ConsultarAsync(
        string cep,
        CancellationToken cancellationToken = default)
        => Task.FromResult<EnderecoCep?>(null);
}
