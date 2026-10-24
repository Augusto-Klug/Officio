using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Officio.Dominio.Interfaces;
using System.Diagnostics;

namespace Officio.Infraestrutura.Servicos;

public class ServicoArmazenamentoAzureBlob(
    BlobServiceClient blobServiceClient,
    ILogger<ServicoArmazenamentoAzureBlob> logger) : IServicoArmazenamento
{
    public Task<string> EnviarAsync(Stream stream, string fileName, string container, CancellationToken ct = default)
        => RegistrarOperacaoAsync("Upload", container, async () =>
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(container);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

            return $"{container}/{fileName}";
        });

    public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
        => RegistrarOperacaoAsync("Download", ObterContainer(path), async () =>
        {
            var parts = path.Split('/', 2);
            var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
            var blobClient = containerClient.GetBlobClient(parts[1]);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
            return response.Value.Content;
        });

    public Task<string> ObterUrlPreAssinadaAsync(string path, TimeSpan expiry, CancellationToken ct = default)
        => RegistrarOperacaoAsync("GerarUrlTemporaria", ObterContainer(path), () =>
        {
            var parts = path.Split('/', 2);
            var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
            var blobClient = containerClient.GetBlobClient(parts[1]);

            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiry));
            return Task.FromResult(sasUri.ToString());
        });

    public async Task ExcluirAsync(string path, CancellationToken ct = default)
        => await RegistrarOperacaoAsync("Excluir", ObterContainer(path), async () =>
        {
            var parts = path.Split('/', 2);
            var containerClient = blobServiceClient.GetBlobContainerClient(parts[0]);
            await containerClient.GetBlobClient(parts[1]).DeleteIfExistsAsync(cancellationToken: ct);
            return true;
        });

    private async Task<T> RegistrarOperacaoAsync<T>(string operacao, string container, Func<Task<T>> executar)
    {
        var timer = Stopwatch.StartNew();
        logger.LogDebug("Iniciando operacao Azure Blob. Operacao={Operacao} Container={Container}", operacao, container);

        try
        {
            var resultado = await executar();
            logger.LogInformation(
                "Operacao Azure Blob concluida. Operacao={Operacao} Container={Container} DuracaoMs={DuracaoMs}",
                operacao,
                container,
                timer.ElapsedMilliseconds);
            return resultado;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Falha na operacao Azure Blob. Operacao={Operacao} Container={Container} TipoExcecao={TipoExcecao} StatusHttp={StatusHttp} DuracaoMs={DuracaoMs} StackTrace={StackTrace}",
                operacao,
                container,
                exception.GetType().Name,
                (exception as RequestFailedException)?.Status,
                timer.ElapsedMilliseconds,
                exception.StackTrace);
            throw;
        }
    }

    private static string ObterContainer(string path)
        => path.Split('/', 2)[0];
}





