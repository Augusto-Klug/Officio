using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Servicos;

internal class ServicoOcrIndisponivel : IServicoOcr
{
    public Task<ResultadoOcr> AnalisarDocumentoAsync(
        Stream documentStream,
        TipoDocumento type,
        CancellationToken ct = default)
        => Task.FromResult(ResultadoOcr.CriarFalha("OCR indisponivel."));
}
