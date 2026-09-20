using System.Buffers;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

internal class DocumentoServico(
    IDocumentoRepositorio documentoRepositorio,
    IServicoOcr servicoOcr,
    IServicoArmazenamento servicoArmazenamento,
    IEmpresaAtual empresaAtual,
    IEstimadorQualidadeDocumento estimadorQualidade) : IDocumentoServico
{
    private const int TamanhoMaximoUploadBytes = 10 * 1024 * 1024;

    public async Task<Resultado<ResultadoEnvioDocumentoDto>> EnviarAsync(
        EnviarDocumentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        if (requisicao.Arquivo.CanSeek)
        {
            requisicao.Arquivo.Position = 0;
            if (requisicao.Arquivo.Length > TamanhoMaximoUploadBytes)
                return CriarFalhaArquivoMuitoGrande();
        }

        var capacidadeInicial = requisicao.Arquivo.CanSeek
            ? (int)requisicao.Arquivo.Length
            : 0;
        using var arquivoEmMemoria = new MemoryStream(capacidadeInicial);
        var excedeuLimite = await CopiarComLimiteAsync(requisicao.Arquivo, arquivoEmMemoria, ct);
        if (excedeuLimite)
            return CriarFalhaArquivoMuitoGrande();

        arquivoEmMemoria.Position = 0;
        var qualidadeEstimada = await estimadorQualidade.EstimarAsync(arquivoEmMemoria, ct);

        if (qualidadeEstimada < 0.4m)
            return Resultado<ResultadoEnvioDocumentoDto>.CriarFalha(
                "Imagem muito escura ou desfocada. Tire uma nova foto com boa iluminacao.");

        arquivoEmMemoria.Position = 0;
        var caminho = await servicoArmazenamento.EnviarAsync(
            arquivoEmMemoria,
            $"documentos/{Guid.NewGuid()}/{requisicao.NomeArquivo}",
            "documents",
            ct);

        var documento = Documento.Create(
            empresaAtual.TenantId,
            requisicao.NomeArquivo,
            requisicao.Tipo,
            caminho,
            qualidadeEstimada,
            requisicao.ClienteId,
            requisicao.ProspectoId,
            requisicao.SocioId,
            requisicao.Observacoes);

        await documentoRepositorio.AdicionarAsync(documento, ct);
        await documentoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<ResultadoEnvioDocumentoDto>.CriarSucesso(
            new ResultadoEnvioDocumentoDto(
                documento.Id,
                documento.NomeArquivo,
                qualidadeEstimada,
                documento.Legivel,
                "Documento salvo. Chame /validate para iniciar a validacao."));
    }

    public async Task<Resultado<ValidationResultDto>> ValidarAsync(
        Guid documentoId,
        CancellationToken ct = default)
    {
        var documento = await documentoRepositorio.ObterPorIdAsync(documentoId, ct);

        if (documento is null)
            return Resultado<ValidationResultDto>.CriarFalha("Documento nao encontrado.");

        if (documento.QualidadeEstimada < 0.4m)
            return Resultado<ValidationResultDto>.CriarFalha(
                "Qualidade do documento abaixo do limite minimo. Reenvie uma imagem mais nitida.");

        var admissaoDocumentoId = await documentoRepositorio.ObterAdmissaoDocumentoIdAsync(documento.Id, ct);

        await using var stream = await servicoArmazenamento.BaixarAsync(documento.CaminhoStorage, ct);
        var resultadoOcr = await servicoOcr.AnalisarDocumentoAsync(stream, documento.Tipo, ct);

        if (!resultadoOcr.Sucesso)
        {
            return await CriarRevisaoManualAsync(
                documento,
                [],
                0m,
                [],
                "OCR indisponivel. Documento marcado para revisao manual.",
                admissaoDocumentoId,
                ct);
        }

        var dadosCadastro = await documentoRepositorio.ObterDadosCadastroAsync(documento, ct);
        if (dadosCadastro.Count == 0)
        {
            return await CriarRevisaoManualAsync(
                documento,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancaOcr,
                [],
                "Dados de cadastro ausentes. Documento marcado para revisao manual.",
                admissaoDocumentoId,
                ct);
        }

        var divergencias = CompararCampos(resultadoOcr.CamposExtraidos, dadosCadastro);
        if (divergencias.Any(divergencia => string.IsNullOrEmpty(divergencia.ValorDocumento)))
        {
            return await CriarRevisaoManualAsync(
                documento,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancaOcr,
                divergencias,
                "Um ou mais campos esperados nao foram identificados pelo OCR. Documento marcado para revisao manual.",
                admissaoDocumentoId,
                ct);
        }

        var valido = divergencias.Count == 0 || divergencias.All(d => d.Similaridade >= 0.85m);
        var validacao = ValidacaoDocumento.Create(
            documento.TenantId,
            documento.Id,
            resultadoOcr.CamposExtraidos,
            divergencias.Cast<object>().ToList(),
            valido,
            resultadoOcr.ConfiancaOcr,
            admissaoDocumentoId);

        await documentoRepositorio.AdicionarValidacaoAsync(validacao, ct);
        await documentoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<ValidationResultDto>.CriarSucesso(
            new ValidationResultDto(
                valido,
                resultadoOcr.ConfiancaOcr,
                divergencias.Select(MapearDivergencia).ToList(),
                false,
                null));
    }

    private async Task<Resultado<ValidationResultDto>> CriarRevisaoManualAsync(
        Documento documento,
        Dictionary<string, string> camposExtraidos,
        decimal confiancaOcr,
        List<DivergenciaDocumento> divergencias,
        string mensagem,
        Guid? admissaoDocumentoId,
        CancellationToken ct)
    {
        var validacao = ValidacaoDocumento.RequireManualReview(
            documento.TenantId,
            documento.Id,
            camposExtraidos,
            confiancaOcr,
            mensagem,
            admissaoDocumentoId);
        await documentoRepositorio.AdicionarValidacaoAsync(validacao, ct);
        await documentoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<ValidationResultDto>.CriarSucesso(
            new ValidationResultDto(
                false,
                confiancaOcr,
                divergencias.Select(MapearDivergencia).ToList(),
                true,
                mensagem));
    }

    private static List<DivergenciaDocumento> CompararCampos(
        Dictionary<string, string> ocr,
        Dictionary<string, string> cadastro)
    {
        var divergencias = new List<DivergenciaDocumento>();

        foreach (var (campo, valorCadastro) in cadastro)
        {
            if (!ocr.TryGetValue(campo, out var valorOcr))
            {
                divergencias.Add(new DivergenciaDocumento(campo, string.Empty, valorCadastro, 0m));
                continue;
            }

            var similaridade = CalcularSimilaridade(
                Normalizar(valorOcr),
                Normalizar(valorCadastro));

            if (similaridade < 0.85m)
                divergencias.Add(new DivergenciaDocumento(campo, valorOcr, valorCadastro, similaridade));
        }

        return divergencias;
    }

    private static async Task<bool> CopiarComLimiteAsync(
        Stream origem,
        MemoryStream destino,
        CancellationToken ct)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        var totalLido = 0;

        try
        {
            int bytesLidos;
            while ((bytesLidos = await origem.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
            {
                totalLido += bytesLidos;
                if (totalLido > TamanhoMaximoUploadBytes)
                    return true;

                await destino.WriteAsync(buffer.AsMemory(0, bytesLidos), ct);
            }

            return false;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static Resultado<ResultadoEnvioDocumentoDto> CriarFalhaArquivoMuitoGrande()
        => Resultado<ResultadoEnvioDocumentoDto>.CriarFalha(
            "Arquivo excede o limite maximo de upload de 10 MB.");

    private static DivergenciaDto MapearDivergencia(DivergenciaDocumento divergencia)
        => new(
            divergencia.Campo,
            divergencia.ValorDocumento,
            divergencia.ValorCadastro,
            divergencia.Similaridade);

    private static decimal CalcularSimilaridade(string a, string b)
    {
        if (a == b) return 1m;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0m;

        var tamanhoMaximo = Math.Max(a.Length, b.Length);
        var distancia = CalcularDistanciaLevenshtein(a, b);
        return 1m - (decimal)distancia / tamanhoMaximo;
    }

    private static int CalcularDistanciaLevenshtein(string a, string b)
    {
        var matriz = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) matriz[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) matriz[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        for (var j = 1; j <= b.Length; j++)
        {
            var custo = a[i - 1] == b[j - 1] ? 0 : 1;
            matriz[i, j] = Math.Min(
                Math.Min(matriz[i - 1, j] + 1, matriz[i, j - 1] + 1),
                matriz[i - 1, j - 1] + custo);
        }

        return matriz[a.Length, b.Length];
    }

    private static string Normalizar(string valor)
        => valor.ToUpperInvariant().Trim()
            .Replace(".", "").Replace("-", "").Replace("/", "");
}
