using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Officio.Dominio.Interfaces;

namespace Officio.Infraestrutura.Servicos;

public class ServicoArmazenamentoAzureBlob(BlobServiceClient blobServiceClient) : IServicoArmazenamento
{
    public async Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

        return $"{container}/{fileName}";
    }

    public async Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
    {
        var parts = path.Split('/', 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
        var blobClient = containerClient.GetBlobClient(parts[1]);

        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }

    public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
    {
        var parts = path.Split('/', 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
        var blobClient = containerClient.GetBlobClient(parts[1]);

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
        return Task.FromResult(sasUri.ToString());
    }

    public async Task ExcluirAsync(string path, CancellationToken ct = default)
    {
        var parts = path.Split('/', 2);
        var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
        await containerClient.GetBlobClient(parts[1]).DeleteIfExistsAsync(cancellationToken: ct);
    }
}





