using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Officio.Dominio.Servicos.Servicos;

public class ContratoServico(
    IContratoRepositorio contratoRepositorio,
    IEmpresaRepositorio empresaRepositorio,
    IOrcamentoRepositorio orcamentoRepositorio,
    IProspectoRepositorio prospectoRepositorio,
    IServicoPdf servicoPdf,
    IServicoAssinatura servicoAssinatura,
    IServicoArmazenamento servicoArmazenamento,
    IEmpresaAtual empresaAtual,
    ILogger<ContratoServico> logger) : IContratoServico
{
    private static readonly HashSet<string> EventosAutentiqueProcessaveis = new(StringComparer.OrdinalIgnoreCase)
    {
        "document.finished",
        "document.updated",
        "signature.accepted",
        "signature.rejected",
        "signature.delivery_failed"
    };

    public async Task<Resultado<Guid>> ConverterOrcamentoAsync(
        ConverterOrcamentoContratoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var orcamento = await orcamentoRepositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
        if (orcamento is null)
            return Resultado<Guid>.CriarFalha("Orcamento nao encontrado.");

        if (orcamento.Status != StatusOrcamento.Aceito)
            return Resultado<Guid>.CriarFalha("Somente orcamentos aceitos podem ser convertidos em contrato.");

        if (await contratoRepositorio.ObterPorOrcamentoAsync(orcamento.Id, ct) is not null)
            return Resultado<Guid>.CriarFalha("Ja existe contrato para este orcamento.");

        var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
        if (prospecto is null)
            return Resultado<Guid>.CriarFalha("Prospecto nao encontrado.");

        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            requisicao.DataInicio,
            requisicao.DataFim,
            requisicao.RenovacaoAutomatica,
            requisicao.ClausulasExtras);

        await contratoRepositorio.AdicionarAsync(contrato, ct);
        await contratoRepositorio.SalvarAlteracoesAsync(ct);

        return Resultado<Guid>.CriarSucesso(contrato.Id);
    }

    public async Task<Resultado<List<ContratoDto>>> ListarAsync(CancellationToken ct = default)
    {
        var contratos = await contratoRepositorio.ListarAsync(ct);
        var dtos = new List<ContratoDto>();
        foreach (var contrato in contratos)
            dtos.Add(await MapearContratoAsync(contrato, ct));

        return Resultado<List<ContratoDto>>.CriarSucesso(dtos);
    }

    public async Task<Resultado<ContratoDto>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default)
    {
        var contrato = await contratoRepositorio.ObterDetalheAsync(contratoId, ct);
        if (contrato is null)
            return Resultado<ContratoDto>.CriarFalha("Contrato nao encontrado.");

        return Resultado<ContratoDto>.CriarSucesso(await MapearContratoAsync(contrato, ct));
    }

    public async Task<Resultado<ResultadoPdfContratoDto>> GerarPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepositorio.ObterParaGeracaoPdfAsync(contratoId, cancellationToken);

        if (contrato is null)
            return Resultado<ResultadoPdfContratoDto>.CriarFalha("Contrato nÃ£o encontrado.");

        var empresa = await empresaRepositorio.ObterPorIdAsync(empresaAtual.TenantId, cancellationToken);
        if (empresa is null)
            return Resultado<ResultadoPdfContratoDto>.CriarFalha("Empresa nÃ£o encontrada.");

        var dadosPdf = MapearDadosPdf(contrato, empresa);

        await using var fluxoPdf = await servicoPdf.GerarContratoAsync(dadosPdf, cancellationToken);

        var caminho = await servicoArmazenamento.EnviarAsync(
            fluxoPdf,
            $"contratos/{contrato.Numero}.pdf",
            "contracts",
            cancellationToken);

        contrato.SetPdfPath(caminho);
        await contratoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        var url = await servicoArmazenamento.ObterUrlPreAssinadaAsync(caminho, TimeSpan.FromHours(1), cancellationToken);

        return Resultado<ResultadoPdfContratoDto>.CriarSucesso(
            new ResultadoPdfContratoDto(url, DateTime.UtcNow));
    }

    public async Task<Resultado<ResultadoPdfAssinadoContratoDto>> ObterPdfAssinadoAsync(
        Guid contratoId,
        CancellationToken ct = default)
    {
        var contrato = await contratoRepositorio.ObterDetalheAsync(contratoId, ct);
        if (contrato is null)
            return Resultado<ResultadoPdfAssinadoContratoDto>.CriarFalha("Contrato nao encontrado.");

        var documentoAssinado = await contratoRepositorio.ObterDocumentoAssinadoAsync(contratoId, ct);
        if (documentoAssinado is null)
            return Resultado<ResultadoPdfAssinadoContratoDto>.CriarFalha("PDF assinado nao encontrado.");

        var expiracao = TimeSpan.FromHours(1);
        var url = await servicoArmazenamento.ObterUrlPreAssinadaAsync(
            documentoAssinado.CaminhoStorage,
            expiracao,
            ct);

        return Resultado<ResultadoPdfAssinadoContratoDto>.CriarSucesso(
            new ResultadoPdfAssinadoContratoDto(url, DateTime.UtcNow.Add(expiracao)));
    }

    public async Task<Resultado<SendForSignatureResultDto>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepositorio.ObterParaAssinaturaAsync(requisicao.ContratoId, cancellationToken);

        if (contrato is null)
            return Resultado<SendForSignatureResultDto>.CriarFalha("Contrato nÃ£o encontrado.");

        if (string.IsNullOrEmpty(contrato.PdfPath))
            return Resultado<SendForSignatureResultDto>.CriarFalha(
                "PDF ainda nÃ£o gerado. Execute a geraÃ§Ã£o de PDF primeiro.");

        await using var fluxoPdf = await servicoArmazenamento.BaixarAsync(contrato.PdfPath, cancellationToken);

        var assinantes = requisicao.Signatarios
            .Select(s => new DadosAssinante(s.Nome, s.Email, s.Cpf, s.Tipo))
            .ToList();

        var solicitacao = new SolicitacaoAssinatura(fluxoPdf, $"{contrato.Numero}.pdf", assinantes);
        var resultado = await servicoAssinatura.EnviarParaAssinaturaAsync(solicitacao, cancellationToken);

        if (resultado.RequerFallback)
        {
            var pdfUrl = await servicoArmazenamento.ObterUrlPreAssinadaAsync(
                contrato.PdfPath,
                TimeSpan.FromHours(24),
                cancellationToken);

            return Resultado<SendForSignatureResultDto>.CriarSucesso(
                new SendForSignatureResultDto("manual_upload", null, null, resultado.Mensagem, pdfUrl));
        }

        contrato.MarcarAguardandoAssinatura();

        var statusAguardando = await contratoRepositorio.ObterOuCriarStatusAssinaturaAsync(
            "aguardando_assinatura",
            "Aguardando assinatura",
            10,
            cancellationToken);

        var solicitacaoAssinatura = AssinaturaSolicitacao.Criar(
            contrato.TenantId,
            contrato.Id,
            statusAguardando.Id,
            "autentique");
        solicitacaoAssinatura.DefinirIdentificadorExterno(resultado.EnvelopeId!);

        var ordemSignatario = 1;
        foreach (var signatario in requisicao.Signatarios)
        {
            solicitacaoAssinatura.AdicionarSignatario(
                signatario.Nome,
                signatario.Email,
                signatario.Tipo,
                ordemSignatario++,
                signatario.Cpf);

            var assinatura = Assinatura.Create(
                contrato.TenantId,
                contrato.Id,
                signatario.Nome,
                signatario.Email,
                signatario.Cpf,
                signatario.Tipo);

            await contratoRepositorio.AdicionarAssinaturaAsync(assinatura, cancellationToken);
        }

        await contratoRepositorio.AdicionarSolicitacaoAssinaturaAsync(solicitacaoAssinatura, cancellationToken);
        await contratoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return Resultado<SendForSignatureResultDto>.CriarSucesso(
            new SendForSignatureResultDto(
                "digital_api",
                resultado.EnvelopeId,
                resultado.LinksAssinantes,
                null,
                null));
    }

    public async Task<Resultado<bool>> ProcessarWebhookAutentiqueAsync(
        string payload,
        CancellationToken ct = default)
    {
        var evento = EventoWebhookAutentique.Criar(payload);
        if (evento is null)
        {
            logger.LogWarning("Webhook Autentique ignorado: nao foi possivel extrair tipo/documento. Payload: {Payload}", payload);
            return Resultado<bool>.CriarFalha("Payload do webhook Autentique invalido.");
        }

        logger.LogInformation(
            "Webhook Autentique recebido. Tipo={Tipo} DocumentoId={DocumentoId} Email={Email} DocumentoFinalizado={DocumentoFinalizado}",
            evento.Tipo,
            evento.DocumentoId,
            evento.EmailSignatario,
            evento.DocumentoFinalizado);

        var solicitacao = await contratoRepositorio.ObterSolicitacaoAssinaturaPorIdExternoAsync(
            "autentique",
            evento.DocumentoId,
            ct);

        if (solicitacao is null)
        {
            logger.LogWarning(
                "Webhook Autentique sem solicitacao correlacionada. Tipo={Tipo} DocumentoId={DocumentoId}",
                evento.Tipo,
                evento.DocumentoId);
            return Resultado<bool>.CriarSucesso(true);
        }

        empresaAtual.SetTenant(solicitacao.TenantId);

        var eventoConclusaoDocumento = EhEventoConclusaoDocumento(evento);
        var eventoJaRegistrado = await contratoRepositorio.ExisteEventoAssinaturaAsync(
                solicitacao.Id,
                evento.Tipo,
                payload,
                ct);
        var documentoAssinadoJaArmazenado = eventoConclusaoDocumento
            && await contratoRepositorio.ExisteDocumentoAssinadoAsync(solicitacao.ContratoId, ct);

        if (eventoJaRegistrado && !eventoConclusaoDocumento)
            return Resultado<bool>.CriarSucesso(true);

        if (eventoJaRegistrado && documentoAssinadoJaArmazenado)
        {
            return Resultado<bool>.CriarSucesso(true);
        }

        if (!eventoJaRegistrado)
        {
            await contratoRepositorio.AdicionarEventoAssinaturaAsync(
                AssinaturaEvento.Criar(
                    solicitacao.TenantId,
                    solicitacao.Id,
                    evento.Tipo,
                    payload,
                    evento.OcorridoEm),
                ct);
        }

        if (!EventosAutentiqueProcessaveis.Contains(evento.Tipo))
        {
            logger.LogInformation(
                "Webhook Autentique registrado sem alteracao de status. Tipo={Tipo} DocumentoId={DocumentoId}",
                evento.Tipo,
                evento.DocumentoId);
            await contratoRepositorio.SalvarAlteracoesAsync(ct);
            return Resultado<bool>.CriarSucesso(true);
        }

        if (eventoConclusaoDocumento)
        {
            var statusAssinado = await contratoRepositorio.ObterOuCriarStatusAssinaturaAsync(
                "assinado",
                "Assinado",
                90,
                ct);

            var ocorridoEm = evento.OcorridoEm ?? DateTime.UtcNow;
            solicitacao.MarcarConcluida(statusAssinado.Id, ocorridoEm);
            if (!solicitacao.Contrato.AssinadoEm.HasValue)
                solicitacao.Contrato.MarkAsSigned(ocorridoEm);

            // A conclusao do contrato nao pode depender da disponibilidade imediata do arquivo.
            await contratoRepositorio.SalvarAlteracoesAsync(ct);

            if (!documentoAssinadoJaArmazenado)
            {
                var armazenamento = await ArmazenarPdfAssinadoAsync(
                    solicitacao,
                    evento.DocumentoId,
                    ct);

                if (!armazenamento.Sucesso)
                    return Resultado<bool>.CriarFalha(armazenamento.Erro!);
            }
        }
        else if (evento.Tipo is "signature.rejected" or "signature.delivery_failed")
        {
            var statusErro = await contratoRepositorio.ObterOuCriarStatusAssinaturaAsync(
                "erro",
                "Erro na assinatura",
                80,
                ct);

            solicitacao.AlterarStatus(statusErro.Id);
        }
        else if (evento.Tipo == "signature.accepted")
        {
            var emailSignatario = evento.EmailSignatario;
            if (!string.IsNullOrWhiteSpace(emailSignatario))
            {
                var signatario = solicitacao.Signatarios.FirstOrDefault(
                    item => string.Equals(item.Email, emailSignatario, StringComparison.OrdinalIgnoreCase));
                signatario?.MarcarAssinado(evento.OcorridoEm ?? DateTime.UtcNow);
            }
        }

        await contratoRepositorio.SalvarAlteracoesAsync(ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private static bool EhEventoConclusaoDocumento(EventoWebhookAutentique evento)
        => string.Equals(evento.Tipo, "document.finished", StringComparison.OrdinalIgnoreCase)
            || (string.Equals(evento.Tipo, "document.updated", StringComparison.OrdinalIgnoreCase)
                && evento.DocumentoFinalizado);

    private async Task<Resultado<bool>> ArmazenarPdfAssinadoAsync(
        AssinaturaSolicitacao solicitacao,
        string documentoId,
        CancellationToken ct)
    {
        var resultadoDownload = await servicoAssinatura.BaixarDocumentoAssinadoAsync(documentoId, ct);
        if (!resultadoDownload.Sucesso || resultadoDownload.Conteudo is null)
            return Resultado<bool>.CriarFalha(
                resultadoDownload.Mensagem ?? "Nao foi possivel baixar o PDF assinado da Autentique.");

        await using var conteudoAssinado = resultadoDownload.Conteudo;
        if (conteudoAssinado.CanSeek)
            conteudoAssinado.Position = 0;

        var nomeArquivo = NormalizarNomeArquivo(
            resultadoDownload.NomeArquivo,
            $"{solicitacao.Contrato.Numero}-assinado.pdf");

        var tamanhoBytes = conteudoAssinado.CanSeek ? conteudoAssinado.Length : (long?)null;
        var caminho = await servicoArmazenamento.EnviarAsync(
            conteudoAssinado,
            $"contratos/{solicitacao.Contrato.Id}/assinaturas/{nomeArquivo}",
            "contracts",
            ct);

        var documento = Documento.CriarContratoAssinado(
            solicitacao.TenantId,
            solicitacao.ContratoId,
            solicitacao.Contrato.EmpresaId,
            solicitacao.Contrato.ClientId,
            solicitacao.Contrato.ProspectId,
            nomeArquivo,
            caminho,
            tamanhoBytes);

        await contratoRepositorio.AdicionarDocumentoAsync(documento, ct);
        return Resultado<bool>.CriarSucesso(true);
    }

    private static string NormalizarNomeArquivo(string? nomeArquivo, string fallback)
    {
        var nome = string.IsNullOrWhiteSpace(nomeArquivo) ? fallback : nomeArquivo.Trim();
        foreach (var caractere in Path.GetInvalidFileNameChars())
            nome = nome.Replace(caractere, '-');

        return nome.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? nome
            : $"{nome}.pdf";
    }

    private static DadosPdfContrato MapearDadosPdf(Contrato contrato, Empresa empresa)
    {
        var cliente = contrato.Cliente;
        var prospecto = contrato.Prospecto;
        var orcamento = contrato.Orcamento;
        var razaoSocial = cliente?.RazaoSocial ?? prospecto.RazaoSocial;
        var nomeFantasia = cliente?.NomeFantasia ?? prospecto.NomeFantasia ?? razaoSocial;
        var cnpj = cliente?.Cnpj ?? prospecto.Cnpj ?? "";
        var regimeTributario = cliente?.RegimeTributario ?? prospecto.RegimeTributario;
        var sociosOrigem = cliente?.Socios ?? prospecto.Socios;

        var contratada = new DadosContratada(
            RazaoSocial: empresa.Nome,
            Cnpj: empresa.Cnpj ?? "",
            Endereco: "",
            Cidade: "",
            Uf: "",
            Email: "",
            Telefone: "");

        var endereco = string.Join(", ",
            new[] { cliente?.Logradouro ?? prospecto.Logradouro, cliente?.Numero ?? prospecto.Numero }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var contratante = new DadosContratante(
            RazaoSocial: razaoSocial,
            NomeFantasia: nomeFantasia,
            Cnpj: cnpj,
            RegimeTributario: regimeTributario?.ToString() ?? "",
            Endereco: endereco,
            Cidade: cliente?.Cidade ?? prospecto.Cidade ?? "",
            Uf: cliente?.Uf ?? prospecto.Uf ?? "",
            Email: cliente?.Email ?? prospecto.Email ?? "",
            Telefone: cliente?.Telefone ?? prospecto.Telefone ?? "");

        var socios = sociosOrigem
            .Select(s => new DadosSocio(s.Nome, s.Cpf ?? "", s.Participacao, Administrador: false))
            .ToList();

        var servicos = orcamento?.Itens
            .Select(i => new DadosServico(i.Descricao, "Mensal", i.ValorUnitario * i.Quantidade))
            .ToList() ?? [];

        return new DadosPdfContrato
        {
            NumeroContrato = contrato.Numero,
            DataEmissao = contrato.DataInicio,
            Versao = 1,
            Contratada = contratada,
            Contratante = contratante,
            Socios = socios,
            Servicos = servicos,
            ValorMensalidade = orcamento?.ValorMensalidade ?? contrato.ValorTotal,
            ValorPorFuncionario = orcamento?.ValorPorFuncionario ?? 0,
            QtdFuncionarios = orcamento?.QtdFuncionarios ?? 0,
            ValorTaxasExtras = orcamento?.ValorTaxasExtras ?? 0,
            Desconto = orcamento?.Desconto ?? 0,
            ValorTotal = contrato.ValorTotal,
            DiaVencimento = contrato.DiaVencimento,
            DataInicio = DateOnly.FromDateTime(contrato.DataInicio),
            DataFim = contrato.DataFim.HasValue
                ? DateOnly.FromDateTime(contrato.DataFim.Value)
                : null,
            RenovacaoAuto = contrato.RenovacaoAutomatica,
            ClausulasExtras = contrato.ClausulasExtras,
        };
    }

    private async Task<ContratoDto> MapearContratoAsync(Contrato contrato, CancellationToken ct)
    {
        var razaoSocial = contrato.Cliente?.RazaoSocial ?? contrato.Prospecto.RazaoSocial;
        var pdfAssinadoDisponivel = await contratoRepositorio.ExisteDocumentoAssinadoAsync(contrato.Id, ct);

        return new ContratoDto(
            contrato.Id,
            contrato.Numero,
            contrato.ProspectId,
            contrato.ClientId,
            contrato.BudgetId,
            razaoSocial,
            contrato.Status.ToString(),
            contrato.ValorTotal,
            contrato.DiaVencimento,
            contrato.DataInicio,
            contrato.DataFim,
            contrato.RenovacaoAutomatica,
            contrato.ClausulasExtras,
            !string.IsNullOrWhiteSpace(contrato.PdfPath),
            pdfAssinadoDisponivel,
            contrato.AssinadoEm);
    }

    private sealed record EventoWebhookAutentique(
        string Tipo,
        string DocumentoId,
        string? EmailSignatario,
        DateTime? OcorridoEm,
        bool DocumentoFinalizado)
    {
        public static EventoWebhookAutentique? Criar(string payload)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;
                var tipo = ObterTipo(root);
                var documentoId = ObterDocumentoId(root);

                if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(documentoId))
                    return null;

                return new EventoWebhookAutentique(
                    tipo,
                    documentoId,
                    ObterEmailSignatario(root),
                    ObterData(root),
                    ObterDocumentoFinalizado(root));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ObterTipo(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            if (TentarObterPropriedade(root, "event", out var eventoObjeto)
                && eventoObjeto.ValueKind == JsonValueKind.Object)
            {
                var tipoEvento = ObterString(eventoObjeto, "type", "name", "event");
                if (!string.IsNullOrWhiteSpace(tipoEvento))
                    return tipoEvento;
            }

            var direto = ObterString(root, "event", "type", "evento", "tipo", "action", "name");
            if (!string.IsNullOrWhiteSpace(direto))
                return direto;

            if (TentarObterPropriedade(root, "data", out var data))
                return ObterTipo(data);

            return null;
        }

        private static string? ObterDocumentoId(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            var direto = ObterString(root, "document_id", "documentId", "document_uuid", "documentUuid");
            if (!string.IsNullOrWhiteSpace(direto))
                return direto;

            if (TentarObterPropriedade(root, "event", out var eventElement) &&
                eventElement.ValueKind == JsonValueKind.Object)
            {
                var documentoId = ObterDocumentoId(eventElement);
                if (!string.IsNullOrWhiteSpace(documentoId))
                    return documentoId;
            }

            if (TentarObterPropriedade(root, "document", out var document))
            {
                var documentoId = ObterDocumentoIdDireto(document);
                if (!string.IsNullOrWhiteSpace(documentoId))
                    return documentoId;
            }

            if (TentarObterPropriedade(root, "object", out var objeto) &&
                objeto.ValueKind == JsonValueKind.Object)
            {
                var objetoDocumentoId = ObterIdObjetoDoTipo(objeto, "document");
                if (!string.IsNullOrWhiteSpace(objetoDocumentoId))
                    return objetoDocumentoId;

                var objetoDireto = ObterString(objeto, "document_id", "documentId", "document_uuid", "documentUuid");
                if (!string.IsNullOrWhiteSpace(objetoDireto))
                    return objetoDireto;

                if (TentarObterPropriedade(objeto, "document", out var objetoDocument))
                {
                    var documentoId = ObterDocumentoIdDireto(objetoDocument);
                    if (!string.IsNullOrWhiteSpace(documentoId))
                        return documentoId;
                }
            }

            if (TentarObterPropriedade(root, "data", out var data) &&
                data.ValueKind == JsonValueKind.Object)
            {
                var dataDocumentoId = ObterIdObjetoDoTipo(data, "document");
                if (!string.IsNullOrWhiteSpace(dataDocumentoId))
                    return dataDocumentoId;

                var dataDireto = ObterString(data, "document_id", "documentId", "document_uuid", "documentUuid");
                if (!string.IsNullOrWhiteSpace(dataDireto))
                    return dataDireto;

                if (TentarObterPropriedade(data, "document", out var dataDocument))
                {
                    var documentoId = ObterDocumentoIdDireto(dataDocument);
                    if (!string.IsNullOrWhiteSpace(documentoId))
                        return documentoId;
                }

                if (TentarObterPropriedade(data, "object", out var dataObject) &&
                    dataObject.ValueKind == JsonValueKind.Object)
                {
                    var objetoDocumentoId = ObterIdObjetoDoTipo(dataObject, "document");
                    if (!string.IsNullOrWhiteSpace(objetoDocumentoId))
                        return objetoDocumentoId;

                    var dataObjectDireto = ObterString(dataObject, "document_id", "documentId", "document_uuid", "documentUuid");
                    if (!string.IsNullOrWhiteSpace(dataObjectDireto))
                        return dataObjectDireto;

                    if (TentarObterPropriedade(dataObject, "document", out var dataObjectDocument))
                    {
                        var documentoId = ObterDocumentoIdDireto(dataObjectDocument);
                        if (!string.IsNullOrWhiteSpace(documentoId))
                            return documentoId;
                    }
                }
            }

            return null;
        }

        private static string? ObterEmailSignatario(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            if (TentarObterPropriedade(root, "signature", out var signature))
                return ObterString(signature, "email");

            if (TentarObterPropriedade(root, "signer", out var signer))
                return ObterString(signer, "email");

            if (TentarObterPropriedade(root, "user", out var user))
                return ObterString(user, "email");

            if (TentarObterPropriedade(root, "event", out var eventElement) &&
                eventElement.ValueKind == JsonValueKind.Object)
            {
                var email = ObterEmailSignatario(eventElement);
                if (!string.IsNullOrWhiteSpace(email))
                    return email;
            }

            if (TentarObterPropriedade(root, "object", out var objeto) &&
                objeto.ValueKind == JsonValueKind.Object)
            {
                var email = ObterEmailSignatario(objeto);
                if (!string.IsNullOrWhiteSpace(email))
                    return email;
            }

            if (TentarObterPropriedade(root, "data", out var data) &&
                data.ValueKind == JsonValueKind.Object)
            {
                var email = ObterEmailSignatario(data);
                if (!string.IsNullOrWhiteSpace(email))
                    return email;
            }

            return ObterString(root, "email", "signer_email", "signerEmail");
        }

        private static DateTime? ObterData(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            var data = ObterString(root, "created_at", "createdAt", "occurred_at", "occurredAt", "created", "signed");
            if (string.IsNullOrWhiteSpace(data) &&
                TentarObterPropriedade(root, "event", out var eventElement) &&
                eventElement.ValueKind == JsonValueKind.Object)
            {
                var dataEvento = ObterData(eventElement);
                if (dataEvento.HasValue)
                    return dataEvento;
            }

            if (string.IsNullOrWhiteSpace(data) && TentarObterPropriedade(root, "data", out var dataElement))
                return ObterData(dataElement);

            return DateTime.TryParse(data, out var valor) ? valor.ToUniversalTime() : null;
        }

        private static bool ObterDocumentoFinalizado(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (DocumentoFinalizadoNoObjeto(root))
                return true;

            if (TentarObterPropriedade(root, "event", out var eventElement) &&
                eventElement.ValueKind == JsonValueKind.Object &&
                ObterDocumentoFinalizado(eventElement))
                return true;

            if (TentarObterPropriedade(root, "data", out var data) &&
                data.ValueKind == JsonValueKind.Object &&
                ObterDocumentoFinalizado(data))
                return true;

            if (TentarObterPropriedade(root, "object", out var objeto) &&
                objeto.ValueKind == JsonValueKind.Object &&
                ObterDocumentoFinalizado(objeto))
                return true;

            return false;
        }

        private static bool DocumentoFinalizadoNoObjeto(JsonElement element)
        {
            var signedFile = ObterArquivoAssinado(element);
            if (!string.IsNullOrWhiteSpace(signedFile))
                return true;

            var signaturesCount = ObterInteiro(element, "signatures_count", "signaturesCount");
            var signedCount = ObterInteiro(element, "signed_count", "signedCount");
            if (signaturesCount > 0 && signedCount >= signaturesCount)
                return true;

            if (!TentarObterPropriedade(element, "signatures", out var signatures) ||
                signatures.ValueKind != JsonValueKind.Array ||
                signatures.GetArrayLength() == 0)
                return false;

            foreach (var signature in signatures.EnumerateArray())
            {
                if (!AssinaturaFinalizada(signature))
                    return false;
            }

            return true;
        }

        private static string? ObterArquivoAssinado(JsonElement element)
        {
            if (!TentarObterPropriedade(element, "files", out var files) ||
                files.ValueKind != JsonValueKind.Object)
                return null;

            return ObterString(files, "signed", "certified", "pades");
        }

        private static bool AssinaturaFinalizada(JsonElement signature)
        {
            if (signature.ValueKind != JsonValueKind.Object)
                return false;

            if (TentarObterPropriedade(signature, "signed", out var signed))
                return signed.ValueKind is JsonValueKind.String or JsonValueKind.Object or JsonValueKind.True;

            return false;
        }

        private static int? ObterInteiro(JsonElement element, params string[] nomes)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var nome in nomes)
            {
                if (!TentarObterPropriedade(element, nome, out var valor))
                    continue;

                if (valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var inteiro))
                    return inteiro;

                if (valor.ValueKind == JsonValueKind.String &&
                    int.TryParse(valor.GetString(), out var inteiroString))
                    return inteiroString;
            }

            return null;
        }

        private static string? ObterString(JsonElement element, params string[] nomes)
        {
            if (element.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var nome in nomes)
            {
                if (TentarObterPropriedade(element, nome, out var valor)
                    && valor.ValueKind == JsonValueKind.String)
                    return valor.GetString();
            }

            return null;
        }

        private static string? ObterDocumentoIdDireto(JsonElement element)
            => element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Object => ObterString(element, "id", "uuid"),
                _ => null
            };

        private static string? ObterIdObjetoDoTipo(JsonElement element, string tipo)
        {
            var objectType = ObterString(element, "object");
            if (!string.Equals(objectType, tipo, StringComparison.OrdinalIgnoreCase))
                return null;

            return ObterString(element, "id", "uuid");
        }

        private static bool TentarObterPropriedade(
            JsonElement element,
            string nome,
            out JsonElement valor)
        {
            if (element.ValueKind == JsonValueKind.Object)
                return element.TryGetProperty(nome, out valor);

            valor = default;
            return false;
        }
    }
}
