using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using Officio.Dominio.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Officio.Dominio.Servicos.Servicos;

internal sealed class EstimadorQualidadeDocumento : IEstimadorQualidadeDocumento
{
    public async Task<decimal> EstimarAsync(Stream arquivo, CancellationToken cancellationToken = default)
    {
        if (!arquivo.CanRead)
            return 0m;

        var posicaoOriginal = arquivo.CanSeek ? arquivo.Position : (long?)null;
        MemoryStream? copiaSeekable = null;

        try
        {
            Stream origem;
            if (arquivo.CanSeek)
            {
                arquivo.Position = 0;
                origem = arquivo;
            }
            else
            {
                copiaSeekable = new MemoryStream();
                await arquivo.CopyToAsync(copiaSeekable, cancellationToken);
                copiaSeekable.Position = 0;
                origem = copiaSeekable;
            }

            if (TentarEstimarBmp(origem, cancellationToken, out var qualidadeBmp))
                return qualidadeBmp;

            origem.Position = 0;
            if (TentarEstimarPgm(origem, cancellationToken, out var qualidadePgm))
                return qualidadePgm;

            origem.Position = 0;
            if (EhPngOuJpeg(origem))
            {
                origem.Position = 0;
                return EstimarImagemComprimida(origem, cancellationToken);
            }

            origem.Position = 0;
            return EstimarFallbackPorBytes(origem, cancellationToken);
        }
        finally
        {
            if (posicaoOriginal.HasValue)
                arquivo.Position = posicaoOriginal.Value;

            copiaSeekable?.Dispose();
        }
    }

    private static bool TentarEstimarBmp(
        Stream arquivo,
        CancellationToken cancellationToken,
        out decimal qualidade)
    {
        qualidade = 0m;
        if (arquivo.Length < 54)
            return false;

        Span<byte> cabecalho = stackalloc byte[54];
        if (!TentarLerExatamente(arquivo, cabecalho)
            || cabecalho[0] != (byte)'B'
            || cabecalho[1] != (byte)'M')
            return false;

        var deslocamentoPixels = BinaryPrimitives.ReadUInt32LittleEndian(cabecalho[10..14]);
        var tamanhoCabecalho = BinaryPrimitives.ReadUInt32LittleEndian(cabecalho[14..18]);
        var largura = BinaryPrimitives.ReadInt32LittleEndian(cabecalho[18..22]);
        var alturaInformada = BinaryPrimitives.ReadInt32LittleEndian(cabecalho[22..26]);
        var planos = BinaryPrimitives.ReadUInt16LittleEndian(cabecalho[26..28]);
        var bitsPorPixel = BinaryPrimitives.ReadUInt16LittleEndian(cabecalho[28..30]);
        var compressao = BinaryPrimitives.ReadUInt32LittleEndian(cabecalho[30..34]);

        if (tamanhoCabecalho < 40
            || largura <= 0
            || alturaInformada == 0
            || alturaInformada == int.MinValue
            || planos != 1
            || bitsPorPixel is not (24 or 32)
            || compressao != 0)
            return false;

        var altura = Math.Abs((long)alturaInformada);
        var bytesPorPixel = bitsPorPixel / 8;
        var bytesPorLinha = ((long)largura * bitsPorPixel + 31) / 32 * 4;
        var tamanhoNecessario = (long)deslocamentoPixels + (bytesPorLinha * altura);
        if (bytesPorLinha > int.MaxValue
            || deslocamentoPixels < 54
            || tamanhoNecessario > arquivo.Length)
            return false;

        arquivo.Position = deslocamentoPixels;
        var linha = ArrayPool<byte>.Shared.Rent((int)bytesPorLinha);
        var metricas = new MetricasPixels();

        try
        {
            for (var y = 0L; y < altura; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!TentarLerExatamente(arquivo, linha.AsSpan(0, (int)bytesPorLinha)))
                    return false;

                for (var x = 0; x < largura; x++)
                {
                    var indice = x * bytesPorPixel;
                    var azul = linha[indice];
                    var verde = linha[indice + 1];
                    var vermelho = linha[indice + 2];
                    var luminancia = (299 * vermelho + 587 * verde + 114 * azul + 500) / 1000;
                    metricas.Adicionar(luminancia);
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(linha);
        }

        qualidade = metricas.CalcularQualidadeVisual();
        return true;
    }

    private static bool TentarEstimarPgm(
        Stream arquivo,
        CancellationToken cancellationToken,
        out decimal qualidade)
    {
        qualidade = 0m;
        var formato = LerToken(arquivo);
        if (formato is not ("P2" or "P5")
            || !TentarLerInteiroPositivo(arquivo, out var largura)
            || !TentarLerInteiroPositivo(arquivo, out var altura)
            || !TentarLerInteiroPositivo(arquivo, out var valorMaximo)
            || valorMaximo > 65535)
            return false;

        var quantidadePixels = (long)largura * altura;
        if (quantidadePixels <= 0 || quantidadePixels > int.MaxValue)
            return false;

        var metricas = new MetricasPixels();
        for (var indice = 0L; indice < quantidadePixels; indice++)
        {
            if ((indice & 4095) == 0)
                cancellationToken.ThrowIfCancellationRequested();

            int amostra;
            if (formato == "P2")
            {
                var token = LerToken(arquivo);
                if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out amostra))
                    return false;
            }
            else if (valorMaximo < 256)
            {
                amostra = arquivo.ReadByte();
                if (amostra < 0)
                    return false;
            }
            else
            {
                var byteAlto = arquivo.ReadByte();
                var byteBaixo = arquivo.ReadByte();
                if (byteAlto < 0 || byteBaixo < 0)
                    return false;

                amostra = (byteAlto << 8) | byteBaixo;
            }

            if (amostra < 0 || amostra > valorMaximo)
                return false;

            metricas.Adicionar((amostra * 255 + (valorMaximo / 2)) / valorMaximo);
        }

        qualidade = metricas.CalcularQualidadeVisual();
        return true;
    }

    private static decimal EstimarFallbackPorBytes(Stream arquivo, CancellationToken cancellationToken)
    {
        // Formatos codificados nao reconhecidos usam entropia dos bytes apenas como fallback.
        var metricas = new MetricasPixels();
        var buffer = ArrayPool<byte>.Shared.Rent(81920);

        try
        {
            int bytesLidos;
            while ((bytesLidos = arquivo.Read(buffer, 0, buffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (var indice = 0; indice < bytesLidos; indice++)
                    metricas.Adicionar(buffer[indice]);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return metricas.CalcularQualidadeFallback();
    }

    private static bool EhPngOuJpeg(Stream arquivo)
    {
        Span<byte> assinatura = stackalloc byte[8];
        var bytesLidos = arquivo.Read(assinatura);

        var png = bytesLidos == 8
                  && assinatura[0] == 137
                  && assinatura[1] == 80
                  && assinatura[2] == 78
                  && assinatura[3] == 71
                  && assinatura[4] == 13
                  && assinatura[5] == 10
                  && assinatura[6] == 26
                  && assinatura[7] == 10;
        var jpeg = bytesLidos >= 3
                   && assinatura[0] == 0xFF
                   && assinatura[1] == 0xD8
                   && assinatura[2] == 0xFF;

        return png || jpeg;
    }

    private static decimal EstimarImagemComprimida(
        Stream arquivo,
        CancellationToken cancellationToken)
    {
        try
        {
            var informacoes = Image.Identify(arquivo);
            if (informacoes is null
                || (long)informacoes.Width * informacoes.Height > 50_000_000)
                return 0m;

            arquivo.Position = 0;
            using var imagem = Image.Load<Rgba32>(arquivo);
            var metricas = new MetricasPixels();

            imagem.ProcessPixelRows(acessor =>
            {
                for (var y = 0; y < acessor.Height; y++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var linha = acessor.GetRowSpan(y);
                    foreach (var pixel in linha)
                    {
                        var luminancia = (299 * pixel.R + 587 * pixel.G + 114 * pixel.B + 500) / 1000;
                        metricas.Adicionar(luminancia);
                    }
                }
            });

            return metricas.CalcularQualidadeVisual();
        }
        catch (UnknownImageFormatException)
        {
            return 0m;
        }
        catch (InvalidImageContentException)
        {
            return 0m;
        }
    }

    private static bool TentarLerInteiroPositivo(Stream arquivo, out int valor)
        => int.TryParse(
            LerToken(arquivo),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out valor)
           && valor > 0;

    private static string? LerToken(Stream arquivo)
    {
        var proximo = arquivo.ReadByte();
        while (proximo >= 0)
        {
            if (proximo == '#')
            {
                while (proximo >= 0 && proximo is not ('\r' or '\n'))
                    proximo = arquivo.ReadByte();
            }
            else if (!char.IsWhiteSpace((char)proximo))
            {
                break;
            }

            proximo = arquivo.ReadByte();
        }

        if (proximo < 0)
            return null;

        var token = new StringBuilder();
        while (proximo >= 0 && !char.IsWhiteSpace((char)proximo) && proximo != '#')
        {
            token.Append((char)proximo);
            proximo = arquivo.ReadByte();
        }

        if (proximo == '\r')
        {
            var possivelNovaLinha = arquivo.ReadByte();
            if (possivelNovaLinha != '\n' && possivelNovaLinha >= 0)
                arquivo.Position--;
        }

        return token.ToString();
    }

    private static bool TentarLerExatamente(Stream arquivo, Span<byte> destino)
    {
        var totalLido = 0;
        while (totalLido < destino.Length)
        {
            var bytesLidos = arquivo.Read(destino[totalLido..]);
            if (bytesLidos == 0)
                return false;

            totalLido += bytesLidos;
        }

        return true;
    }

    private sealed class MetricasPixels
    {
        private readonly long[] histograma = new long[256];
        private long quantidade;
        private double soma;
        private double somaQuadrados;

        public void Adicionar(int luminancia)
        {
            histograma[luminancia]++;
            quantidade++;
            soma += luminancia;
            somaQuadrados += luminancia * luminancia;
        }

        public decimal CalcularQualidadeVisual()
        {
            if (quantidade == 0)
                return 0m;

            var mediaBrilho = soma / quantidade / 255d;
            var mediaBruta = soma / quantidade;
            var variancia = Math.Max(0d, (somaQuadrados / quantidade) - (mediaBruta * mediaBruta));
            var contraste = Math.Clamp(Math.Sqrt(variancia) / 127.5d, 0d, 1d);
            var entropia = CalcularEntropiaNormalizada();
            var qualidade = (mediaBrilho * 0.30d) + (contraste * 0.45d) + (entropia * 0.25d);

            return ArredondarQualidade(qualidade);
        }

        public decimal CalcularQualidadeFallback()
        {
            if (quantidade == 0)
                return 0m;

            var mediaBytes = soma / quantidade / 255d;
            var qualidade = (mediaBytes * 0.35d) + (CalcularEntropiaNormalizada() * 0.65d);
            return ArredondarQualidade(qualidade);
        }

        private double CalcularEntropiaNormalizada()
        {
            var entropia = 0d;
            foreach (var ocorrencias in histograma)
            {
                if (ocorrencias == 0)
                    continue;

                var probabilidade = (double)ocorrencias / quantidade;
                entropia -= probabilidade * Math.Log2(probabilidade);
            }

            return entropia / 8d;
        }

        private static decimal ArredondarQualidade(double qualidade)
            => Math.Clamp(decimal.Round((decimal)qualidade, 2), 0m, 1m);
    }
}
