using System.Collections.Concurrent;
using Officio.Dominio.Interfaces;

namespace Officio.Testes.Integracao.Infraestrutura;

public sealed class ServicoArmazenamentoFake : IServicoArmazenamento
{
    private readonly ConcurrentDictionary<string, byte[]> arquivos = new();

    public async Task<string> EnviarAsync(
        Stream stream,
        string fileName,
        string container,
        CancellationToken ct = default)
    {
        var caminho = $"{container}/{fileName}";
        await using var memoria = new MemoryStream();
        await stream.CopyToAsync(memoria, ct);
        arquivos[caminho] = memoria.ToArray();
        return caminho;
    }

    public Task<Stream> BaixarAsync(string path, CancellationToken ct = default)
        => arquivos.TryGetValue(path, out var conteudo)
            ? Task.FromResult<Stream>(new MemoryStream(conteudo, writable: false))
            : throw new FileNotFoundException("Arquivo nao encontrado no armazenamento de teste.", path);

    public Task<string> ObterUrlPreAssinadaAsync(
        string path,
        TimeSpan expiry,
        CancellationToken ct = default)
        => Task.FromResult($"https://storage.officio.test/{Uri.EscapeDataString(path)}");

    public Task ExcluirAsync(string path, CancellationToken ct = default)
    {
        arquivos.TryRemove(path, out _);
        return Task.CompletedTask;
    }
}
