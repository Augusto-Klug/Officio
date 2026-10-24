using Microsoft.Extensions.Logging;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Eventos;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class AdmissaoDocumentoServico(
    IAdmissaoRepositorio repositorio,
    IDocumentoServico documentoServico,
    IServicoArmazenamento armazenamento,
    IEstimadorQualidadeDocumento estimadorQualidade,
    IConversorImagensPdf conversorImagensPdf,
    IEmpresaAtual empresaAtual,
    IUsuarioAtual usuarioAtual,
    ILogger<AdmissaoDocumentoServico> logger,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal) : IAdmissaoDocumentoServico
{
    private const int TamanhoMaximoUploadBytes = 10 * 1024 * 1024;
    private const int QuantidadeMaximaArquivos = 10;
    private static readonly HashSet<string> TiposPermitidos =
        ["application/pdf", "image/jpeg", "image/png"];
    private static readonly IReadOnlyDictionary<string, string[]> ExtensoesPorMime =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = [".pdf"],
            ["image/jpeg"] = [".jpg", ".jpeg"],
            ["image/png"] = [".png"]
        };

    public async Task<Resultado<List<AdmissaoDocumentoDto>>> ListarAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterDetalheAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<List<AdmissaoDocumentoDto>>.CriarFalha("Admissao nao encontrada.");

        var documentos = await repositorio.ListarDocumentosAsync(admissaoId, ct);
        return Resultado<List<AdmissaoDocumentoDto>>.CriarSucesso(documentos.Select(Mapear).ToList());
    }

    public async Task<Resultado<ResultadoDocumentoAdmissaoDto>> EnviarAsync(
        EnviarDocumentoAdmissaoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterDocumentoAsync(
            requisicao.AdmissaoId, requisicao.AdmissaoDocumentoId, rastreamento: true, ct);
        var erro = ValidarItemEditavel(item);
        if (erro is not null)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(erro);
        if (!TentarMapearTipo(item!.TipoDocumento.Codigo, out var tipoDocumento))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "Tipo de documento de admissao nao suportado para upload.");
        if (requisicao.Arquivos.Count is 0 or > QuantidadeMaximaArquivos)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                $"Envie de 1 a {QuantidadeMaximaArquivos} arquivos por documento.");

        var arquivos = new List<(string Nome, string Mime, byte[] Conteudo, decimal Qualidade)>();
        var totalBytes = 0L;
        var totalBytesLidos = 0L;
        foreach (var arquivo in requisicao.Arquivos)
        {
            var mime = arquivo.MimeType.Trim().ToLowerInvariant();
            var nome = Path.GetFileName(arquivo.NomeArquivo);
            if (arquivo.TamanhoBytes <= 0 || string.IsNullOrWhiteSpace(nome))
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Um dos arquivos enviados esta vazio ou sem nome.");
            if (!TiposPermitidos.Contains(mime))
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Formato nao permitido. Envie PDF, JPG ou PNG.");
            if (!ExtensoesPorMime[mime].Contains(Path.GetExtension(nome), StringComparer.OrdinalIgnoreCase))
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                    "A extensao de um arquivo nao corresponde ao formato informado.");

            totalBytes += arquivo.TamanhoBytes;
            if (arquivo.TamanhoBytes > TamanhoMaximoUploadBytes || totalBytes > TamanhoMaximoUploadBytes)
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                    "O tamanho total dos arquivos excede o limite de 10 MB.");

            await using var conteudo = new MemoryStream((int)arquivo.TamanhoBytes);
            if (await CopiarComLimiteAsync(arquivo.Conteudo, conteudo, TamanhoMaximoUploadBytes, ct))
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Um dos arquivos excede o limite de 10 MB.");
            var bytes = conteudo.ToArray();
            totalBytesLidos += bytes.Length;
            if (totalBytesLidos > TamanhoMaximoUploadBytes)
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                    "O tamanho total dos arquivos excede o limite de 10 MB.");
            decimal qualidade;
            try
            {
                qualidade = mime == "application/pdf"
                    ? 1m
                    : await EstimarQualidadeAsync(bytes, ct);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(
                    "Falha ao estimar qualidade da imagem do documento. ItemId={ItemId} AdmissaoId={AdmissaoId} TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                    item.Id,
                    item.AdmissaoId,
                    exception.GetType().FullName,
                    exception.StackTrace);
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                    "Uma das imagens esta invalida ou em formato nao suportado. Confira o arquivo e tente novamente.");
            }
            if (qualidade < 0.4m)
                return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                    "Uma das imagens esta muito escura ou desfocada. Envie imagens legiveis.");
            arquivos.Add((nome, mime, bytes, qualidade));
        }

        if (arquivos.Count > 1 && arquivos.Any(arquivo => arquivo.Mime == "application/pdf"))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "Para combinar paginas, selecione somente imagens JPG ou PNG. Um PDF deve ser enviado sozinho.");

        var gerarPdf = arquivos.Count > 1;
        byte[] bytesFinais;
        try
        {
            bytesFinais = gerarPdf
                ? conversorImagensPdf.CriarPdf(arquivos.Select(arquivo => arquivo.Conteudo).ToList())
                : arquivos[0].Conteudo;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                "Falha ao combinar imagens do documento de admissao. AdmissaoId={AdmissaoId} TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                item!.AdmissaoId,
                exception.GetType().FullName,
                exception.StackTrace);
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "Nao foi possivel ler uma das imagens. Confira os arquivos e tente novamente.");
        }

        if (bytesFinais.Length > TamanhoMaximoUploadBytes)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "O documento combinado excede o limite de 10 MB. Reduza o tamanho das imagens.");

        var mimeType = gerarPdf ? "application/pdf" : arquivos[0].Mime;
        var nomeArquivo = gerarPdf
            ? $"{Path.GetFileNameWithoutExtension(arquivos[0].Nome)}-multipagina.pdf"
            : arquivos[0].Nome;
        var qualidadeFinal = arquivos.Min(arquivo => arquivo.Qualidade);
        AdmissaoDocumento? alternativaSubstituida = null;
        Documento? documentoAlternativo = null;
        if (item.TipoDocumento.Codigo is "IDENTIDADE" or "CNH")
        {
            var admissao = await repositorio.ObterParaAlteracaoAsync(item.AdmissaoId, ct);
            alternativaSubstituida = admissao?.Documentos.FirstOrDefault(candidato =>
                candidato.Id != item.Id &&
                (candidato.TipoDocumento.Codigo is "IDENTIDADE" or "CNH") &&
                candidato.Documento is not null);
            documentoAlternativo = alternativaSubstituida?.Documento;
        }

        await using var memoria = new MemoryStream(bytesFinais, writable: false);
        var nomeStorage = $"{Guid.NewGuid():N}-{NormalizarNomeArquivo(nomeArquivo)}";
        var caminho = await armazenamento.EnviarAsync(
            memoria,
            $"admissoes/{empresaAtual.TenantId}/{item!.AdmissaoId}/{item.Id}/{nomeStorage}",
            "documents",
            ct);

        var documentoAnterior = item.Documento;
        var documento = Documento.CriarAdmissao(
            empresaAtual.TenantId,
            item.AdmissaoId,
            item.Admissao.EmpresaId,
            item.TipoDocumentoId,
            nomeArquivo,
            tipoDocumento,
            caminho,
            mimeType,
            memoria.Length,
            qualidadeFinal);

        try
        {
            item.VincularDocumento(documento);
            await repositorio.AdicionarDocumentoAsync(documento, ct);
            if (documentoAnterior is not null)
                await repositorio.RemoverDocumentoAsync(documentoAnterior, ct);
            if (alternativaSubstituida is not null && documentoAlternativo is not null)
            {
                alternativaSubstituida.RemoverDocumento();
                await repositorio.RemoverDocumentoAsync(documentoAlternativo, ct);
            }
            await repositorio.SalvarAlteracoesAsync(ct);
            await PublicarAtualizacaoAsync(item, ct);
        }
        catch
        {
            await TentarExcluirAsync(caminho, ct);
            throw;
        }

        if (documentoAnterior is not null)
            await TentarExcluirAsync(documentoAnterior.CaminhoStorage, ct);
        if (documentoAlternativo is not null)
            await TentarExcluirAsync(documentoAlternativo.CaminhoStorage, ct);

        var atualizado = await repositorio.ObterDocumentoAsync(item.AdmissaoId, item.Id, rastreamento: false, ct);
        return Resultado<ResultadoDocumentoAdmissaoDto>.CriarSucesso(
            new ResultadoDocumentoAdmissaoDto(Mapear(atualizado!), "Documento enviado com sucesso."));
    }

    public async Task<Resultado<ResultadoUrlDocumentoAdmissaoDto>> ObterUrlAsync(
        Guid admissaoId,
        Guid admissaoDocumentoId,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterDocumentoAsync(admissaoId, admissaoDocumentoId, rastreamento: false, ct);
        if (item is null)
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha("Documento da admissao nao encontrado.");
        if (item.Documento is null)
            return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarFalha("Arquivo ainda nao enviado.");

        var expiracao = TimeSpan.FromMinutes(15);
        var url = await armazenamento.ObterUrlPreAssinadaAsync(item.Documento.CaminhoStorage, expiracao, ct);
        return Resultado<ResultadoUrlDocumentoAdmissaoDto>.CriarSucesso(
            new ResultadoUrlDocumentoAdmissaoDto(url, DateTime.UtcNow.Add(expiracao)));
    }

    public async Task<Resultado<ValidationResultDto>> ValidarAsync(
        Guid admissaoId,
        Guid admissaoDocumentoId,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterDocumentoAsync(admissaoId, admissaoDocumentoId, rastreamento: true, ct);
        var erro = ValidarItemEditavel(item);
        if (erro is not null)
            return Resultado<ValidationResultDto>.CriarFalha(erro);
        if (item!.Admissao.StatusAdmissao.Codigo != StatusAdmissaoCodigos.AguardandoValidacao)
            return Resultado<ValidationResultDto>.CriarFalha("A validacao documental so pode ocorrer enquanto a admissao aguarda revisao da contabilidade.");
        if (item.DocumentoId is null)
            return Resultado<ValidationResultDto>.CriarFalha("Envie o arquivo antes de iniciar a validacao.");

        var resultado = await documentoServico.ValidarAsync(item.DocumentoId.Value, ct);
        if (!resultado.Sucesso)
            return resultado;

        await PublicarAtualizacaoAsync(item, ct);
        return resultado;
    }

    public async Task<Resultado<bool>> AprovarManualmenteAsync(
        AprovarDocumentoAdmissaoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterDocumentoAsync(
            requisicao.AdmissaoId, requisicao.AdmissaoDocumentoId, rastreamento: true, ct);
        var erro = ValidarItemEditavel(item);
        if (erro is not null)
            return Resultado<bool>.CriarFalha(erro);
        if (item!.Admissao.StatusAdmissao.Codigo != StatusAdmissaoCodigos.AguardandoValidacao)
            return Resultado<bool>.CriarFalha("A revisao manual so pode ocorrer enquanto a admissao aguarda revisao da contabilidade.");
        if (item.DocumentoId is null)
            return Resultado<bool>.CriarFalha("Arquivo ainda nao enviado.");
        if (string.IsNullOrWhiteSpace(requisicao.Observacao))
            return Resultado<bool>.CriarFalha("Informe a justificativa da revisao manual.");
        if (usuarioAtual.UserId == Guid.Empty)
            return Resultado<bool>.CriarFalha("Nao foi possivel identificar o usuario revisor.");

        var validacao = await repositorio.ObterUltimaValidacaoParaAlteracaoAsync(item.DocumentoId.Value, ct);
        if (validacao is null)
            return Resultado<bool>.CriarFalha("Validacao do documento nao encontrada.");
        if (validacao.Status is not (StatusValidacao.Reprovado or StatusValidacao.RevisaoManual))
            return Resultado<bool>.CriarFalha("Somente documentos reprovados ou em revisao manual podem ser aprovados manualmente.");
        if (validacao.TipoDocumentoIncompativel)
            return Resultado<bool>.CriarFalha("O tipo do documento nao corresponde ao item da admissao. Substitua o arquivo pelo documento correto.");

        validacao.ApproveManually(usuarioAtual.UserId, requisicao.Observacao);
        await repositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(item, ct);

        return Resultado<bool>.CriarSucesso(true);
    }

    public async Task<Resultado<bool>> RemoverAsync(
        Guid admissaoId,
        Guid admissaoDocumentoId,
        CancellationToken ct = default)
    {
        var item = await repositorio.ObterDocumentoAsync(admissaoId, admissaoDocumentoId, rastreamento: true, ct);
        var erro = ValidarItemEditavel(item);
        if (erro is not null)
            return Resultado<bool>.CriarFalha(erro);
        if (item!.Documento is null)
            return Resultado<bool>.CriarSucesso(true);

        var documento = item.Documento;
        item.RemoverDocumento();
        await repositorio.RemoverDocumentoAsync(documento, ct);
        await repositorio.SalvarAlteracoesAsync(ct);
        await PublicarAtualizacaoAsync(item, ct);
        await TentarExcluirAsync(documento.CaminhoStorage, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private Task PublicarAtualizacaoAsync(AdmissaoDocumento item, CancellationToken ct)
        => publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(
                item.Admissao.TenantId,
                EntidadesAtualizacaoTempoReal.Admissao,
                item.AdmissaoId),
            ct);

    private static string? ValidarItemEditavel(AdmissaoDocumento? item)
    {
        if (item is null)
            return "Documento da admissao nao encontrado.";
        var status = item.Admissao.StatusAdmissao.Codigo;
        if (StatusAdmissaoCodigos.Terminal(status))
            return "Documentos de uma admissao concluida ou cancelada nao podem ser alterados.";
        return status is StatusAdmissaoCodigos.Rascunho
            or StatusAdmissaoCodigos.AguardandoAdmitido
            or StatusAdmissaoCodigos.AguardandoValidacao
                ? null
                : "Documentos nao podem ser alterados apos a conclusao da revisao documental.";
    }

    private static AdmissaoDocumentoDto Mapear(AdmissaoDocumento item)
    {
        var validacao = item.Documento?.Validacoes
            .OrderByDescending(registro => registro.CriadoEm)
            .FirstOrDefault();
        var status = item.Documento is null
            ? "PENDENTE_ENVIO"
            : validacao?.Status switch
            {
                StatusValidacao.Aprovado => "APROVADO",
                StatusValidacao.Reprovado => "REPROVADO",
                StatusValidacao.RevisaoManual => "REVISAO_MANUAL",
                StatusValidacao.Pendente => "PENDENTE_VALIDACAO",
                _ => "ENVIADO"
            };

        return new(
            item.Id, item.TipoDocumentoId, item.TipoDocumento.Codigo, item.TipoDocumento.Descricao,
            item.RotuloExibicao ?? item.TipoDocumento.Descricao, item.Obrigatorio, item.Ordem,
            item.DocumentoId, item.Documento?.NomeArquivo, item.Documento?.MimeType,
            item.Documento?.TamanhoBytes, item.Documento?.QualidadeEstimada, item.Documento?.CriadoEm,
            status, validacao?.ConfiancaOcr, validacao?.Observacao,
            validacao?.CamposExtraidos.Where(campo => !campo.Key.StartsWith("__", StringComparison.Ordinal))
                .ToDictionary(campo => campo.Key, campo => campo.Value) ?? [],
            validacao?.Divergencias.Select(divergencia => new DivergenciaDto(
                divergencia.CodigoCampo,
                divergencia.ValorExtraido ?? string.Empty,
                divergencia.ValorInformado ?? string.Empty,
                divergencia.Confianca ?? 0m)).ToList() ?? [],
            validacao?.TipoDocumentoDetectado,
            validacao?.TipoDocumentoIncompativel ?? false,
            validacao?.ObservacaoRevisao,
            validacao?.RevisadoPor?.Nome,
            validacao?.RevisadoEm,
            validacao?.OcrExecucao?.StatusProcessamento,
            validacao?.OcrExecucao?.CategoriaResultado,
            validacao?.OcrExecucao?.ModeloUtilizado,
            validacao?.OcrExecucao?.StatusHttp,
            validacao?.OcrExecucao?.CodigoErro,
            validacao?.OcrExecucao?.IdExterno,
            validacao?.OcrExecucao?.Campos.ToDictionary(campo => campo.CodigoCampo, campo => campo.Confianca ?? 0m) ?? [],
            validacao?.OcrExecucao?.PayloadRetorno);
    }

    private static bool TentarMapearTipo(string codigo, out TipoDocumento tipo)
    {
        tipo = codigo switch
        {
            "IDENTIDADE" => TipoDocumento.Identidade,
            "CPF" => TipoDocumento.Cpf,
            "COMPROVANTE_RESIDENCIA" => TipoDocumento.ComprovanteResidencia,
            "CARTEIRA_TRABALHO" => TipoDocumento.CarteiraTrabalho,
            "CNH" => TipoDocumento.Cnh,
            "TITULO_ELEITOR" => TipoDocumento.TituloEleitor,
            "COMPROVANTE_ESCOLARIDADE" => TipoDocumento.ComprovanteEscolaridade,
            "CERTIFICADO_RESERVISTA" => TipoDocumento.CertificadoReservista,
            _ => (TipoDocumento)(-1)
        };
        return (int)tipo >= 0;
    }

    private static string NormalizarNomeArquivo(string nomeArquivo)
    {
        var nome = string.Concat(nomeArquivo.Select(caractere =>
            char.IsLetterOrDigit(caractere) || caractere is '.' or '-' or '_' ? caractere : '_'));
        return string.IsNullOrWhiteSpace(nome) ? "documento" : nome;
    }

    private static async Task<bool> CopiarComLimiteAsync(
        Stream origem,
        Stream destino,
        int limiteBytes,
        CancellationToken ct)
    {
        var buffer = new byte[81920];
        var total = 0;
        int lidos;
        while ((lidos = await origem.ReadAsync(buffer, ct)) > 0)
        {
            total += lidos;
            if (total > limiteBytes)
                return true;

            await destino.WriteAsync(buffer.AsMemory(0, lidos), ct);
        }

        return false;
    }

    private async Task<decimal> EstimarQualidadeAsync(byte[] conteudo, CancellationToken ct)
    {
        await using var memoria = new MemoryStream(conteudo, writable: false);
        return await estimadorQualidade.EstimarAsync(memoria, ct);
    }

    private async Task TentarExcluirAsync(string caminho, CancellationToken ct)
    {
        try
        {
            await armazenamento.ExcluirAsync(caminho, ct);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Nao foi possivel excluir arquivo temporario do armazenamento. TipoExcecao={TipoExcecao}",
                exception.GetType().Name);
        }
    }
}
