namespace ContaFacil.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string container, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task<string> GetPresignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default);
}
