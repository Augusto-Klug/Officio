using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Servicos;

namespace Officio.Testes.Unitarios.DominioServicos;

public class EstimadorQualidadeDocumentoTestes
{
    [Fact]
    public async Task EstimarAsync_QuandoBmpPossuiBrilhoEContraste_AprovaQualidade()
    {
        await using var provedor = CriarProvedor();
        var estimador = provedor.GetRequiredService<IEstimadorQualidadeDocumento>();
        await using var imagem = new MemoryStream(CriarBmp24Contrastado());

        var qualidade = await estimador.EstimarAsync(imagem);

        Assert.True(qualidade >= 0.4m, $"Qualidade calculada: {qualidade}");
    }

    [Fact]
    public async Task EstimarAsync_QuandoPgmP5PossuiBrilhoEContraste_AprovaQualidade()
    {
        await using var provedor = CriarProvedor();
        var estimador = provedor.GetRequiredService<IEstimadorQualidadeDocumento>();
        await using var imagem = new MemoryStream(CriarPgmP5Contrastado());

        var qualidade = await estimador.EstimarAsync(imagem);

        Assert.True(qualidade >= 0.4m, $"Qualidade calculada: {qualidade}");
    }

    [Fact]
    public async Task EstimarAsync_QuandoPgmP2EstaEscuro_RejeitaQualidade()
    {
        await using var provedor = CriarProvedor();
        var estimador = provedor.GetRequiredService<IEstimadorQualidadeDocumento>();
        await using var imagem = new MemoryStream(CriarPgmP2Escuro());

        var qualidade = await estimador.EstimarAsync(imagem);

        Assert.True(qualidade < 0.4m, $"Qualidade calculada: {qualidade}");
    }

    private static ServiceProvider CriarProvedor()
    {
        var servicos = new ServiceCollection();
        servicos.AdicionarServicosDominio(new ConfigurationBuilder().Build());
        return servicos.BuildServiceProvider();
    }

    private static byte[] CriarBmp24Contrastado()
    {
        const int largura = 16;
        const int altura = 16;
        const int bytesPorLinha = largura * 3;
        const int tamanhoPixels = bytesPorLinha * altura;

        using var memoria = new MemoryStream();
        using var escritor = new BinaryWriter(memoria);
        escritor.Write((ushort)0x4D42);
        escritor.Write(54 + tamanhoPixels);
        escritor.Write(0);
        escritor.Write(54);
        escritor.Write(40);
        escritor.Write(largura);
        escritor.Write(altura);
        escritor.Write((ushort)1);
        escritor.Write((ushort)24);
        escritor.Write(0);
        escritor.Write(tamanhoPixels);
        escritor.Write(2835);
        escritor.Write(2835);
        escritor.Write(0);
        escritor.Write(0);

        for (var y = 0; y < altura; y++)
        for (var x = 0; x < largura; x++)
        {
            var intensidade = (byte)(((x / 2) + (y / 2)) % 2 == 0 ? 32 : 240);
            escritor.Write(intensidade);
            escritor.Write(intensidade);
            escritor.Write(intensidade);
        }

        return memoria.ToArray();
    }

    private static byte[] CriarPgmP5Contrastado()
    {
        var cabecalho = System.Text.Encoding.ASCII.GetBytes("P5\n16 16\n255\n");
        var imagem = new byte[cabecalho.Length + 256];
        cabecalho.CopyTo(imagem, 0);

        for (var i = 0; i < 256; i++)
            imagem[cabecalho.Length + i] = (byte)(i % 2 == 0 ? 24 : 232);

        return imagem;
    }

    private static byte[] CriarPgmP2Escuro()
    {
        var pixels = string.Join(' ', Enumerable.Repeat("4", 256));
        return System.Text.Encoding.ASCII.GetBytes($"P2\n16 16\n255\n{pixels}\n");
    }
}
