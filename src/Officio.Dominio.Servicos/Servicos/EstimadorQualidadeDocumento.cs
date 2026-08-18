using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class EstimadorQualidadeDocumento : IEstimadorQualidadeDocumento
{
    public async Task<decimal> EstimarAsync(Stream arquivo, CancellationToken cancellationToken = default)
    {
        if (arquivo.CanSeek)
            arquivo.Position = 0;

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, cancellationToken);
        var bytes = memoria.ToArray();

        if (arquivo.CanSeek)
            arquivo.Position = 0;

        if (bytes.Length == 0)
            return 0m;

        var mediaBrilho = (decimal)bytes.Average(b => b) / 255m;
        var distintos = bytes.Distinct().Count();
        var diversidade = Math.Min(1m, distintos / 128m);
        var qualidade = (mediaBrilho * 0.45m) + (diversidade * 0.55m);

        return Math.Clamp(decimal.Round(qualidade, 2), 0m, 1m);
    }
}
