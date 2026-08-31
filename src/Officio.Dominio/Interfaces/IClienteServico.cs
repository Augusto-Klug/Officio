using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;

namespace Officio.Dominio.Interfaces;

public interface IClienteServico
{
    Task<Resultado<List<ClienteDto>>> ListarAsync(
        string? busca = null,
        bool? ativo = null,
        CancellationToken ct = default);

    Task<Resultado<ClienteDetalheDto>> ObterPorIdAsync(Guid clienteId, CancellationToken ct = default);

    Task<Resultado<Guid>> AtualizarAsync(AtualizarClienteRequisicao requisicao, CancellationToken ct = default);

    Task<Resultado<bool>> VincularCodigoSciAsync(VincularCodigoSciRequisicao requisicao, CancellationToken ct = default);
}
