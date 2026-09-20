namespace Officio.Dominio.Interfaces;

public interface IServicoArmazenamento
{
    Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default);
    Task<Stream> BaixarAsync(string path, CancellationToken ct = default);
    Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default);
    Task ExcluirAsync(string path, CancellationToken ct = default);
}





