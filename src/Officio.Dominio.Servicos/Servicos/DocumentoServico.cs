using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Servicos.Servicos;

internal class DocumentoServico(
    DbContext db,
    IServicoOcr servicoOcr,
    IServicoArmazenamento servicoArmazenamento,
    IEmpresaAtual empresaAtual) : IDocumentoServico
{
    public async Task<ResultadoOperacao<EnvioDocumentoResultado>> EnviarAsync(
        EnviarDocumentoRequisicao requisicao,
        CancellationToken ct = default)
    {
        // TODO: substituir por anÃ¡lise real de qualidade da imagem.
        var qualidadeEstimada = 0.85m;

        if (qualidadeEstimada < 0.4m)
            return ResultadoOperacao<EnvioDocumentoResultado>.CriarFalha(
                "Imagem muito escura ou desfocada. Tire uma nova foto com boa iluminaÃ§Ã£o.");

        var caminho = await servicoArmazenamento.EnviarAsync(
            requisicao.Arquivo,
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

        db.Set<Documento>().Add(documento);
        await db.SaveChangesAsync(ct);

        return ResultadoOperacao<EnvioDocumentoResultado>.CriarSucesso(
            new EnvioDocumentoResultado(
                documento.Id,
                documento.NomeArquivo,
                qualidadeEstimada,
                documento.Legivel,
                "Documento salvo. Chame /validate para iniciar a validaÃ§Ã£o."));
    }

    public async Task<ResultadoOperacao<ValidacaoDocumentoResultado>> ValidarAsync(
        Guid documentoId,
        CancellationToken ct = default)
    {
        var documento = await db.Set<Documento>()
            .FirstOrDefaultAsync(d => d.Id == documentoId, ct);

        if (documento is null)
            return ResultadoOperacao<ValidacaoDocumentoResultado>.CriarFalha("Documento nÃ£o encontrado.");

        if (documento.QualidadeEstimada < 0.4m)
            return ResultadoOperacao<ValidacaoDocumentoResultado>.CriarFalha(
                "Qualidade do documento abaixo do limite mÃ­nimo. Reenvie uma imagem mais nÃ­tida.");

        await using var stream = await servicoArmazenamento.BaixarAsync(documento.CaminhoStorage, ct);

        var resultadoOcr = await servicoOcr.AnalisarDocumentoAsync(stream, documento.Tipo, ct);

        if (!resultadoOcr.Sucesso)
        {
            var fallback = ValidacaoDocumento.RequireManualReview(documento.Id);
            db.Set<ValidacaoDocumento>().Add(fallback);
            await db.SaveChangesAsync(ct);

            return ResultadoOperacao<ValidacaoDocumentoResultado>.CriarSucesso(
                new ValidacaoDocumentoResultado(
                    false,
                    0,
                    [],
                    true,
                    "OCR indisponÃ­vel. Documento marcado para revisÃ£o manual."));
        }

        var divergencias = CompararCampos(resultadoOcr.CamposExtraidos, new Dictionary<string, string>());
        var valido = divergencias.Count == 0 || divergencias.All(d => d.Similaridade >= 0.85m);

        var validacao = ValidacaoDocumento.Create(
            documento.Id,
            resultadoOcr.CamposExtraidos,
            divergencias.Cast<object>().ToList(),
            valido,
            resultadoOcr.ConfiancaOcr);

        db.Set<ValidacaoDocumento>().Add(validacao);
        await db.SaveChangesAsync(ct);

        return ResultadoOperacao<ValidacaoDocumentoResultado>.CriarSucesso(
            new ValidacaoDocumentoResultado(valido, resultadoOcr.ConfiancaOcr, divergencias, false, null));
    }

    private static List<DivergenciaDocumento> CompararCampos(
        Dictionary<string, string> ocr,
        Dictionary<string, string> cadastro)
    {
        var divergencias = new List<DivergenciaDocumento>();

        foreach (var (campo, valorCadastro) in cadastro)
        {
            if (!ocr.TryGetValue(campo, out var valorOcr))
                continue;

            var similaridade = CalcularSimilaridade(
                Normalizar(valorOcr),
                Normalizar(valorCadastro));

            if (similaridade < 0.85m)
                divergencias.Add(new DivergenciaDocumento(campo, valorOcr, valorCadastro, similaridade));
        }

        return divergencias;
    }

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
