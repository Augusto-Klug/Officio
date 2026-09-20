using Microsoft.Extensions.Logging;
using Officio.Dominio.Admissoes;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;

namespace Officio.Dominio.Servicos.Servicos;

public sealed class AdmissaoDocumentoServico(
    IAdmissaoRepositorio repositorio,
    IDocumentoServico documentoServico,
    IServicoArmazenamento armazenamento,
    IEstimadorQualidadeDocumento estimadorQualidade,
    IEmpresaAtual empresaAtual,
    ILogger<AdmissaoDocumentoServico> logger) : IAdmissaoDocumentoServico
{
    private const int TamanhoMaximoUploadBytes = 10 * 1024 * 1024;
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
        if (requisicao.TamanhoBytes <= 0)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Arquivo nao informado.");
        if (requisicao.TamanhoBytes > TamanhoMaximoUploadBytes)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Arquivo excede o limite maximo de 10 MB.");

        var mimeType = requisicao.MimeType.Trim().ToLowerInvariant();
        if (!TiposPermitidos.Contains(mimeType))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Formato nao permitido. Envie PDF, JPG ou PNG.");

        var nomeArquivo = Path.GetFileName(requisicao.NomeArquivo);
        if (string.IsNullOrWhiteSpace(nomeArquivo))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Nome do arquivo nao informado.");

        var extensao = Path.GetExtension(nomeArquivo);
        if (!ExtensoesPorMime[mimeType].Contains(extensao, StringComparer.OrdinalIgnoreCase))
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "A extensao do arquivo nao corresponde ao formato informado.");

        await using var memoria = new MemoryStream((int)Math.Min(requisicao.TamanhoBytes, TamanhoMaximoUploadBytes));
        var excedeuLimite = await CopiarComLimiteAsync(requisicao.Arquivo, memoria, ct);
        if (excedeuLimite)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha("Arquivo excede o limite maximo de 10 MB.");

        memoria.Position = 0;
        var qualidade = mimeType == "application/pdf"
            ? 1m
            : await estimadorQualidade.EstimarAsync(memoria, ct);
        if (qualidade < 0.4m)
            return Resultado<ResultadoDocumentoAdmissaoDto>.CriarFalha(
                "Imagem muito escura ou desfocada. Envie uma nova imagem com boa iluminacao.");

        var nomeStorage = $"{Guid.NewGuid():N}-{NormalizarNomeArquivo(nomeArquivo)}";
        memoria.Position = 0;
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
            MapearTipo(item.TipoDocumento.Codigo),
            caminho,
            mimeType,
            memoria.Length,
            qualidade);

        try
        {
            item.VincularDocumento(documento);
            await repositorio.AdicionarDocumentoAsync(documento, ct);
            if (documentoAnterior is not null)
                await repositorio.RemoverDocumentoAsync(documentoAnterior, ct);
            await repositorio.SalvarAlteracoesAsync(ct);
        }
        catch
        {
            await TentarExcluirAsync(caminho, ct);
            throw;
        }

        if (documentoAnterior is not null)
            await TentarExcluirAsync(documentoAnterior.CaminhoStorage, ct);

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
        if (item!.DocumentoId is null)
            return Resultado<ValidationResultDto>.CriarFalha("Envie o arquivo antes de iniciar a validacao.");

        var resultado = await documentoServico.ValidarAsync(item.DocumentoId.Value, ct);
        if (!resultado.Sucesso)
            return resultado;

        if (resultado.Valor!.FallbackManual || !resultado.Valor.Valido)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(
                StatusAdmissaoCodigos.PendenteRevisao, "Pendente de revisao", 4, ct);
            item.Admissao.AlterarStatus(status.Id);
            await repositorio.SalvarAlteracoesAsync(ct);
        }

        return resultado;
    }

    public async Task<Resultado<ResumoValidacaoDocumentosAdmissaoDto>> ValidarPendentesAsync(
        Guid admissaoId,
        CancellationToken ct = default)
    {
        var admissao = await repositorio.ObterDetalheAsync(admissaoId, ct);
        if (admissao is null)
            return Resultado<ResumoValidacaoDocumentosAdmissaoDto>.CriarFalha("Admissao nao encontrada.");

        var documentos = await repositorio.ListarDocumentosAsync(admissaoId, ct);
        var pendentes = documentos
            .Where(item => item.DocumentoId.HasValue && item.Documento!.Validacoes.Count == 0)
            .ToList();
        var aprovados = 0;
        var pendentesRevisao = 0;
        var falhas = 0;

        foreach (var item in pendentes)
        {
            try
            {
                var resultado = await ValidarAsync(admissaoId, item.Id, ct);
                if (!resultado.Sucesso)
                {
                    falhas++;
                    logger.LogWarning(
                        "Nao foi possivel validar automaticamente o documento {DocumentoId} da admissao {AdmissaoId}: {Erro}",
                        item.DocumentoId, admissaoId, resultado.Erro);
                    continue;
                }

                if (resultado.Valor!.Valido && !resultado.Valor.FallbackManual)
                    aprovados++;
                else
                    pendentesRevisao++;
            }
            catch (Exception exception)
            {
                falhas++;
                logger.LogWarning(exception,
                    "Falha ao validar automaticamente o documento {DocumentoId} da admissao {AdmissaoId}.",
                    item.DocumentoId, admissaoId);
            }
        }

        if (falhas > 0)
        {
            var admissaoParaAlteracao = await repositorio.ObterParaAlteracaoAsync(admissaoId, ct);
            if (admissaoParaAlteracao is not null)
            {
                var status = await repositorio.ObterOuCriarStatusAsync(
                    StatusAdmissaoCodigos.PendenteRevisao, "Pendente de revisao", 4, ct);
                admissaoParaAlteracao.AlterarStatus(status.Id);
                await repositorio.SalvarAlteracoesAsync(ct);
            }
        }

        return Resultado<ResumoValidacaoDocumentosAdmissaoDto>.CriarSucesso(
            new(pendentes.Count, aprovados, pendentesRevisao, falhas));
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
        if (item!.DocumentoId is null)
            return Resultado<bool>.CriarFalha("Arquivo ainda nao enviado.");

        var validacao = await repositorio.ObterUltimaValidacaoParaAlteracaoAsync(item.DocumentoId.Value, ct);
        if (validacao is null)
            return Resultado<bool>.CriarFalha("Validacao do documento nao encontrada.");

        validacao.ApproveManually(requisicao.Observacao);
        await repositorio.SalvarAlteracoesAsync(ct);

        var documentos = await repositorio.ListarDocumentosAsync(item.AdmissaoId, ct);
        var possuiOutraRevisao = documentos.Any(documento =>
            documento.Documento?.Validacoes.OrderByDescending(v => v.CriadoEm).FirstOrDefault()?.Status
                is StatusValidacao.Reprovado or StatusValidacao.RevisaoManual);
        if (!possuiOutraRevisao)
        {
            var status = await repositorio.ObterOuCriarStatusAsync(StatusAdmissaoCodigos.Rascunho, "Rascunho", 1, ct);
            item.Admissao.AlterarStatus(status.Id);
            await repositorio.SalvarAlteracoesAsync(ct);
        }

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
        await TentarExcluirAsync(documento.CaminhoStorage, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private static string? ValidarItemEditavel(AdmissaoDocumento? item)
    {
        if (item is null)
            return "Documento da admissao nao encontrado.";
        return StatusAdmissaoCodigos.Terminal(item.Admissao.StatusAdmissao.Codigo)
            ? "Documentos de uma admissao concluida ou cancelada nao podem ser alterados."
            : null;
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
            validacao?.CamposExtraidos ?? [],
            validacao?.Divergencias.Select(divergencia => new DivergenciaDto(
                divergencia.CodigoCampo,
                divergencia.ValorExtraido ?? string.Empty,
                divergencia.ValorInformado ?? string.Empty,
                divergencia.Confianca ?? 0m)).ToList() ?? []);
    }

    private static TipoDocumento MapearTipo(string codigo)
        => codigo switch
        {
            "IDENTIDADE" => TipoDocumento.Identidade,
            "CPF" => TipoDocumento.Cpf,
            "COMPROVANTE_RESIDENCIA" => TipoDocumento.ComprovanteResidencia,
            "CARTEIRA_TRABALHO" => TipoDocumento.CarteiraTrabalho,
            _ => TipoDocumento.Identidade
        };

    private static string NormalizarNomeArquivo(string nomeArquivo)
    {
        var nome = string.Concat(nomeArquivo.Select(caractere =>
            char.IsLetterOrDigit(caractere) || caractere is '.' or '-' or '_' ? caractere : '_'));
        return string.IsNullOrWhiteSpace(nome) ? "documento" : nome;
    }

    private static async Task<bool> CopiarComLimiteAsync(Stream origem, Stream destino, CancellationToken ct)
    {
        var buffer = new byte[81920];
        var total = 0;
        int lidos;
        while ((lidos = await origem.ReadAsync(buffer, ct)) > 0)
        {
            total += lidos;
            if (total > TamanhoMaximoUploadBytes)
                return true;

            await destino.WriteAsync(buffer.AsMemory(0, lidos), ct);
        }

        return false;
    }

    private async Task TentarExcluirAsync(string caminho, CancellationToken ct)
    {
        try
        {
            await armazenamento.ExcluirAsync(caminho, ct);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Nao foi possivel excluir o arquivo {Caminho} do armazenamento.", caminho);
        }
    }
}
