using System.Buffers;
using System.Globalization;
using System.Text;
using Officio.Dominio.Contratos;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Entidades;
using Officio.Dominio.Enumeradores;
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

        var admissaoDocumentoId = await documentoRepositorio.ObterAdmissaoDocumentoIdAsync(documento.Id, ct);
        if (documento.QualidadeEstimada < 0.4m)
        {
            const string mensagemQualidade = "A imagem esta escura ou desfocada. Substitua o arquivo ou solicite revisao manual.";
            if (admissaoDocumentoId.HasValue)
                return await CriarRevisaoManualAsync(
                    documento, [], 0m, [], mensagemQualidade, admissaoDocumentoId, null, ct);

            return Resultado<ValidationResultDto>.CriarFalha(mensagemQualidade);
        }

        await using var stream = await servicoArmazenamento.BaixarAsync(documento.CaminhoStorage, ct);
        var resultadoOcr = await servicoOcr.AnalisarDocumentoAsync(stream, documento.Tipo, ct);
        var execucaoOcr = OcrExecucao.Criar(documento.Id, "AZURE_DOCUMENT_INTELLIGENCE");
        if (resultadoOcr.Sucesso)
        {
            execucaoOcr.Concluir(
                resultadoOcr.ModeloUtilizado,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancasCampos,
                resultadoOcr.IdRequisicao,
                resultadoOcr.StatusHttp);
        }
        else
        {
            execucaoOcr.Falhar(
                resultadoOcr.ModeloUtilizado,
                resultadoOcr.Erro,
                resultadoOcr.CategoriaResultado,
                resultadoOcr.CodigoErro,
                resultadoOcr.StatusHttp,
                resultadoOcr.IdRequisicao,
                resultadoOcr.DiagnosticoTecnico);
        }

        await documentoRepositorio.AdicionarExecucaoOcrAsync(execucaoOcr, ct);

        if (!resultadoOcr.Sucesso)
        {
            return await CriarRevisaoManualAsync(
                documento,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancaOcr,
                [],
                resultadoOcr.Erro ?? "OCR nao conseguiu processar o documento.",
                admissaoDocumentoId,
                execucaoOcr.Id,
                ct,
                resultadoOcr.DiagnosticoTecnico);
        }

        var tipoDetectado = resultadoOcr.CamposExtraidos.GetValueOrDefault("tipo_documento_detectado");
        if (admissaoDocumentoId.HasValue)
        {
            var tipoEsperado = await documentoRepositorio.ObterTipoDocumentoAdmissaoAsync(admissaoDocumentoId.Value, ct);
            if (tipoEsperado is null || tipoDetectado is null)
            {
                return await CriarRevisaoManualAsync(
                    documento, resultadoOcr.CamposExtraidos, resultadoOcr.ConfiancaOcr, [],
                    "Nao foi possivel confirmar automaticamente o tipo do documento. Confirme visualmente.",
                    admissaoDocumentoId, execucaoOcr.Id, ct);
            }

            if (!TipoDocumentoCompativel(tipoEsperado, tipoDetectado))
            {
                const string mensagemTipo = "O documento enviado nao corresponde ao tipo solicitado. Substitua pelo documento correto.";
                var camposAuditoria = CriarCamposAuditoria(resultadoOcr.CamposExtraidos, tipoDetectado, incompatibilidade: true);
                var validacaoTipo = ValidacaoDocumento.Create(
                    documento.TenantId, documento.Id, camposAuditoria, [], false,
                    resultadoOcr.ConfiancaOcr, admissaoDocumentoId, execucaoOcr.Id, mensagemTipo);
                await documentoRepositorio.AdicionarValidacaoAsync(validacaoTipo, ct);
                await documentoRepositorio.SalvarAlteracoesAsync(ct);

                return Resultado<ValidationResultDto>.CriarSucesso(
                    new ValidationResultDto(false, resultadoOcr.ConfiancaOcr, [], false, mensagemTipo));
            }
        }

        if (documento.Tipo == TipoDocumento.TituloEleitor &&
            (!resultadoOcr.CamposExtraidos.TryGetValue("numero_titulo", out var numeroTitulo) ||
             string.Concat(numeroTitulo.Where(char.IsDigit)).Length != 12))
        {
            return await CriarRevisaoManualAsync(
                documento,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancaOcr,
                [],
                "Numero de inscricao do titulo eleitoral nao identificado. Documento marcado para revisao manual.",
                admissaoDocumentoId,
                execucaoOcr.Id,
                ct);
        }

        var dadosCadastroCompletos = await documentoRepositorio.ObterDadosCadastroAsync(documento, ct);
        var dadosCadastro = SelecionarDadosComparaveis(
            documento.Tipo,
            CriarCamposAuditoria(resultadoOcr.CamposExtraidos, tipoDetectado),
            dadosCadastroCompletos);
        if (dadosCadastro.Count == 0)
        {
            return await CriarRevisaoManualAsync(
                documento,
                resultadoOcr.CamposExtraidos,
                resultadoOcr.ConfiancaOcr,
                [],
                "Dados de cadastro ausentes. Documento marcado para revisao manual.",
                admissaoDocumentoId,
                execucaoOcr.Id,
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
                CriarMensagemCamposNaoIdentificados(divergencias),
                admissaoDocumentoId,
                execucaoOcr.Id,
                ct);
        }

        var valido = divergencias.Count == 0 || divergencias.All(d => d.Similaridade >= 0.85m);
        var validacao = ValidacaoDocumento.Create(
            documento.TenantId,
            documento.Id,
            resultadoOcr.CamposExtraidos,
            divergencias,
            valido,
            resultadoOcr.ConfiancaOcr,
            admissaoDocumentoId,
            execucaoOcr.Id);

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
        Guid? ocrExecucaoId,
        CancellationToken ct,
        string? diagnosticoTecnicoOcr = null)
    {
        var validacao = ValidacaoDocumento.RequireManualReview(
            documento.TenantId,
            documento.Id,
            camposExtraidos,
            confiancaOcr,
            mensagem,
            admissaoDocumentoId,
            divergencias,
            ocrExecucaoId);
        await documentoRepositorio.AdicionarValidacaoAsync(validacao, ct);
        await documentoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<ValidationResultDto>.CriarSucesso(
            new ValidationResultDto(
                false,
                confiancaOcr,
                divergencias.Select(MapearDivergencia).ToList(),
                true,
                mensagem,
                diagnosticoTecnicoOcr));
    }

    private static string CriarMensagemCamposNaoIdentificados(IEnumerable<DivergenciaDocumento> divergencias)
    {
        var campos = divergencias
            .Where(divergencia => string.IsNullOrWhiteSpace(divergencia.ValorDocumento))
            .Select(divergencia => divergencia.Campo switch
            {
            "nome" => "nome",
                "cpf" => "CPF",
                "rg" => "RG",
                "data_nascimento" => "data de nascimento",
            "numero_titulo" => "numero do titulo",
                _ => divergencia.Campo.Replace('_', ' ')
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (campos.Count == 0)
            return "Nao foi possivel ler alguns dados do documento. Confira se o arquivo esta legivel ou solicite uma conferencia manual.";

        var camposFormatados = campos.Count == 1
            ? campos[0]
            : $"{string.Join(", ", campos.Take(campos.Count - 1))} e {campos[^1]}";
        return $"Nao conseguimos identificar {camposFormatados} neste documento. Confira se o arquivo esta legivel e corresponde ao tipo selecionado.";
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

    private static Dictionary<string, string> CriarCamposAuditoria(
        IReadOnlyDictionary<string, string> campos,
        string? tipoDetectado,
        bool incompatibilidade = false)
    {
        var resultado = campos.ToDictionary(campo => campo.Key, campo => campo.Value, StringComparer.OrdinalIgnoreCase);
        if (tipoDetectado is not null)
            resultado["__tipo_documento_detectado"] = tipoDetectado;
        if (incompatibilidade)
            resultado["__tipo_documento_incompativel"] = "true";
        return resultado;
    }

    private static bool TipoDocumentoCompativel(string codigoEsperado, string detectado)
    {
        var esperado = codigoEsperado.ToUpperInvariant();
        var tipo = detectado.ToUpperInvariant();
        if (esperado is "IDENTIDADE" or "RG") esperado = "IDENTIDADE";
        return esperado == tipo;
    }

    private static Dictionary<string, string> SelecionarDadosComparaveis(
        TipoDocumento tipo,
        IReadOnlyDictionary<string, string> camposExtraidos,
        IReadOnlyDictionary<string, string> dadosCadastro)
    {
        var perfil = PerfilValidacaoDocumento.Obter(tipo);
        var selecionados = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var campo in perfil.Obrigatorios)
        {
            if (dadosCadastro.TryGetValue(campo, out var valor))
                selecionados[campo] = valor;
        }

        var alternativasDisponiveis = perfil.PeloMenosUm
            .Where(dadosCadastro.ContainsKey)
            .ToList();
        var alternativasExtraidas = alternativasDisponiveis
            .Where(camposExtraidos.ContainsKey)
            .ToList();
        var alternativasSelecionadas = alternativasExtraidas.Count > 0
            ? alternativasExtraidas
            : alternativasDisponiveis.Take(1);
        foreach (var campo in alternativasSelecionadas)
            selecionados[campo] = dadosCadastro[campo];

        foreach (var campo in perfil.Opcionais)
        {
            if (camposExtraidos.ContainsKey(campo) && dadosCadastro.TryGetValue(campo, out var valor))
                selecionados[campo] = valor;
        }

        return selecionados;
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
    {
        var decomposicao = valor.Trim().Normalize(NormalizationForm.FormD);
        var normalizado = new StringBuilder(decomposicao.Length);

        foreach (var caractere in decomposicao)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(caractere))
                normalizado.Append(char.ToUpperInvariant(caractere));
        }

        return normalizado.ToString();
    }

    private sealed record PerfilValidacaoDocumento(
        IReadOnlyCollection<string> Obrigatorios,
        IReadOnlyCollection<string> PeloMenosUm,
        IReadOnlyCollection<string> Opcionais)
    {
        private static readonly PerfilValidacaoDocumento PessoaComCpf =
            new(["nome", "cpf"], [], ["data_nascimento"]);

        public static PerfilValidacaoDocumento Obter(TipoDocumento tipo)
            => tipo switch
            {
                TipoDocumento.CartaoCnpj or TipoDocumento.ContratoSocial =>
                    new(["razao_social", "cnpj"], [], ["nome_fantasia", "email"]),
                TipoDocumento.RgSocio => new(["nome"], ["cpf"], []),
                TipoDocumento.CpfSocio => new(["cpf"], [], ["nome"]),
                TipoDocumento.CnhSocio or TipoDocumento.Cnh => PessoaComCpf,
                TipoDocumento.Identidade => new(["nome"], ["cpf", "rg", "data_nascimento"], []),
                TipoDocumento.Cpf => new(["cpf"], [], ["nome"]),
                TipoDocumento.ComprovanteResidencia => new(["nome"], [], []),
                TipoDocumento.CarteiraTrabalho => PessoaComCpf,
                TipoDocumento.TituloEleitor => new(["nome"], [], ["data_nascimento"]),
                TipoDocumento.ComprovanteEscolaridade => new(["nome"], [], ["escolaridade"]),
                TipoDocumento.CertificadoReservista => PessoaComCpf,
                _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de documento sem perfil de validacao.")
            };
    }
}
