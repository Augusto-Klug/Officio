using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ContaFacil.Application.Common.Interfaces;

namespace ContaFacil.Infrastructure.Services;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient) : IStorageService
{
    public async Task<string> UploadAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

        return $"{container}/{fileName}";
    }

    public async Task<Stream> DownloadAsync(string path, CancellationToken ct = default)
    {
        var parts = path.Split('/', 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
        var blobClient = containerClient.GetBlobClient(parts[1]);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public Task<string> GetPresignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        var parts = path.Split('/', 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
        var blobClient = containerClient.GetBlobClient(parts[1]);

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sasUri.ToString());
    }
}
