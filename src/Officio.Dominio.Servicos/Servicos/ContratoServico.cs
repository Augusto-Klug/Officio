using Officio.Dominio.Entidades;
using Officio.Dominio.Eventos;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Officio.Dominio.Comum;
using Officio.Dominio.Servicos.Validadores;
using Officio.Dominio.Servicos.Mapeadores;

namespace Officio.Dominio.Servicos.Servicos;

public class ContratoServico(
    IContratoRepositorio contratoRepositorio,
    IContabilidadeRepositorio contabilidadeRepositorio,
    IOrcamentoRepositorio orcamentoRepositorio,
    IProspectoRepositorio prospectoRepositorio,
    IServicoEnderecoComercial servicoEnderecoComercial,
    IServicoPdf servicoPdf,
    IServicoAssinatura servicoAssinatura,
    IServicoArmazenamento servicoArmazenamento,
    IEmpresaAtual empresaAtual,
    IPublicadorAtualizacaoTempoReal publicadorAtualizacaoTempoReal,
    ILogger<ContratoServico> logger,
    TimeProvider timeProvider) : IContratoServico
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

        if (orcamento.TenantId != empresaAtual.TenantId)
            return Resultado<Guid>.CriarFalha("Orcamento nao encontrado.");

        if (await contratoRepositorio.ObterPorOrcamentoAsync(orcamento.Id, ct) is not null)
            return Resultado<Guid>.CriarFalha("Ja existe contrato para este orcamento.");

        var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
        if (prospecto is null || prospecto.Id != orcamento.ProspectId || prospecto.TenantId != empresaAtual.TenantId)
            return Resultado<Guid>.CriarFalha("Prospecto nao encontrado.");

        var cliente = await prospectoRepositorio.ObterClientePorProspectoAsync(prospecto.Id, ct);
        var socios = prospecto.Socios.ToList();
        if (cliente is not null && cliente.TenantId == prospecto.TenantId)
            socios.AddRange(await prospectoRepositorio.ObterSociosClienteAsync(cliente.Id, ct));
        var representante = requisicao.RepresentanteSocioId.HasValue
            ? socios.FirstOrDefault(s => s.Id == requisicao.RepresentanteSocioId && s.TenantId == prospecto.TenantId
                && (s.ProspectId == prospecto.Id || (cliente is not null && s.ClientId == cliente.Id)))
            : null;

        var cnpjProspecto = DocumentoFiscal.SomenteDigitos(prospecto.Cnpj);
        var empresaExistente = cnpjProspecto?.Length == 14
            ? await prospectoRepositorio.ObterEmpresaPorCnpjAsync(cnpjProspecto, ct)
            : null;
        var contratada = await contabilidadeRepositorio.ObterAtualAsync(ct);
        var pendencias = ValidadorProntidaoContrato.Validar(
            empresaAtual.TenantId,
            contratada,
            prospecto,
            empresaExistente,
            orcamento,
            requisicao.RepresentanteSocioId,
            representante,
            requisicao.DataInicio,
            requisicao.DuracaoAnos,
            requisicao.CompetenciaInicialHonorarios);

        if (pendencias.Count > 0)
        {
            return Resultado<Guid>.CriarFalhaValidacao(
                pendencias,
                ContratoErros.DadosObrigatoriosPendentes);
        }

        var empresa = await GarantirEmpresaDoProspectoAsync(prospecto, empresaExistente, ct);
        if (!empresa.Sucesso)
            return Resultado<Guid>.CriarFalha(empresa.Erro!);

        var contratosAtivos = await contratoRepositorio.ListarAtivosPorEmpresaAsync(empresa.Valor!.Id, ct);
        if (contratosAtivos.Count > 0 && !requisicao.ConfirmarSubstituicaoContratoAtivo)
            return Resultado<Guid>.CriarFalha(ContratoErros.SubstituicaoRequerConfirmacao);

        var contrato = Contrato.CriarComRepresentante(
            orcamento,
            prospecto,
            representante!,
            cliente?.Id,
            requisicao.DataInicio,
            requisicao.DuracaoAnos,
            requisicao.CompetenciaInicialHonorarios,
            requisicao.ClausulasExtras);

        await contratoRepositorio.AdicionarAsync(contrato, ct);
        await contratoRepositorio.SalvarAlteracoesAsync(ct);
        await publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(contrato.TenantId, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id),
            ct);

        return Resultado<Guid>.CriarSucesso(contrato.Id);
    }

    public async Task<Resultado<RepresentantesContratoDto>> ObterRepresentantesAsync(Guid orcamentoId, CancellationToken ct = default)
    {
        var orcamento = await orcamentoRepositorio.ObterPorIdComItensAsync(orcamentoId, ct);
        if (orcamento is null || orcamento.TenantId != empresaAtual.TenantId)
            return Resultado<RepresentantesContratoDto>.CriarFalha("Orcamento nao encontrado.");
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
        if (prospecto is null || prospecto.TenantId != empresaAtual.TenantId)
            return Resultado<RepresentantesContratoDto>.CriarFalha("Prospecto nao encontrado.");
        var cliente = await prospectoRepositorio.ObterClientePorProspectoAsync(prospecto.Id, ct);
        var socios = prospecto.Socios.ToList();
        if (cliente is not null && cliente.TenantId == prospecto.TenantId)
            socios.AddRange(await prospectoRepositorio.ObterSociosClienteAsync(cliente.Id, ct));
        var dtos = socios.Where(s => s.TenantId == prospecto.TenantId)
            .DistinctBy(s => s.PessoaId).Select(s =>
            {
                var faltantes = CadastroRepresentante.InformacoesFaltantes(s);
                return new RepresentanteDisponivelDto(s.Id, s.Nome, s.Cpf, s.TipoRelacao?.Descricao, faltantes.Count == 0, faltantes);
            }).ToList();
        return Resultado<RepresentantesContratoDto>.CriarSucesso(new(prospecto.Id, dtos));
    }

    public async Task<Resultado<List<ContratoAtivoResumoDto>>> ListarAtivosParaOrcamentoAsync(
        Guid orcamentoId,
        CancellationToken ct = default)
    {
        var orcamento = await orcamentoRepositorio.ObterPorIdComItensAsync(orcamentoId, ct);
        if (orcamento is null)
            return Resultado<List<ContratoAtivoResumoDto>>.CriarFalha("Orcamento nao encontrado.");

        var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
        if (prospecto is null)
            return Resultado<List<ContratoAtivoResumoDto>>.CriarFalha("Prospecto nao encontrado.");

        var empresaId = prospecto.EmpresaId;
        if (!empresaId.HasValue && !string.IsNullOrWhiteSpace(prospecto.Cnpj))
        {
            var empresa = await prospectoRepositorio.ObterEmpresaPorCnpjAsync(prospecto.Cnpj, ct);
            empresaId = empresa?.Id;
        }

        if (!empresaId.HasValue)
            return Resultado<List<ContratoAtivoResumoDto>>.CriarSucesso([]);

        var contratos = await contratoRepositorio.ListarAtivosPorEmpresaAsync(empresaId.Value, ct);
        return Resultado<List<ContratoAtivoResumoDto>>.CriarSucesso(contratos
            .Select(contrato => new ContratoAtivoResumoDto(
                contrato.Id,
                contrato.Numero,
                contrato.DataInicio,
                contrato.DataFim))
            .ToList());
    }

    public async Task<Resultado<List<ContratoDto>>> ListarAsync(
        Guid? empresaId = null,
        StatusContrato? status = null,
        CancellationToken ct = default)
    {
        var contratos = await contratoRepositorio.ListarAsync(empresaId, status, ct);
        var idsComPdfAssinado = await contratoRepositorio.ListarIdsComDocumentoAssinadoAsync(
            contratos.Select(contrato => contrato.Id).ToList(),
            ct);
        var dtos = contratos
            .Select(contrato => MapearContrato(
                contrato,
                idsComPdfAssinado.Contains(contrato.Id)))
            .ToList();

        return Resultado<List<ContratoDto>>.CriarSucesso(dtos);
    }

    public async Task<Resultado<int>> EncerrarContratosExpiradosAsync(
        DateOnly dataReferencia,
        CancellationToken ct = default)
    {
        var limite = dataReferencia.ToDateTime(TimeOnly.MinValue);
        var expirados = await contratoRepositorio.ListarContratosExpiradosAsync(limite, ct);

        foreach (var grupoTenant in expirados.GroupBy(contrato => contrato.TenantId))
        {
            empresaAtual.SetTenant(grupoTenant.Key);
            foreach (var contrato in grupoTenant)
                contrato.Encerrar();

            await contratoRepositorio.SalvarAlteracoesAsync(ct);
            foreach (var contrato in grupoTenant)
                await publicadorAtualizacaoTempoReal.PublicarAsync(
                    new EventoAtualizacaoTempoReal(grupoTenant.Key, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id),
                    ct);
        }

        logger.LogInformation(
            "Encerramento diario de contratos expirados concluido. DataReferencia={DataReferencia} ContratosEncerrados={Quantidade}",
            dataReferencia,
            expirados.Count);

        return Resultado<int>.CriarSucesso(expirados.Count);
    }

    public async Task<Resultado<List<EmpresaContratoFiltroDto>>> ListarEmpresasAsync(
        CancellationToken ct = default)
    {
        var empresas = await contratoRepositorio.ListarEmpresasComContratoAsync(ct);
        return Resultado<List<EmpresaContratoFiltroDto>>.CriarSucesso(empresas
            .Select(empresa => new EmpresaContratoFiltroDto(
                empresa.Id,
                empresa.RazaoSocial,
                empresa.Cnpj))
            .ToList());
    }

    public async Task<Resultado<ContratoDto>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default)
    {
        var contrato = await contratoRepositorio.ObterDetalheAsync(contratoId, ct);
        if (contrato is null)
            return Resultado<ContratoDto>.CriarFalha("Contrato nao encontrado.");

        var pdfAssinadoDisponivel = await contratoRepositorio.ExisteDocumentoAssinadoAsync(contrato.Id, ct);
        return Resultado<ContratoDto>.CriarSucesso(MapearContrato(contrato, pdfAssinadoDisponivel));
    }

    public async Task<Resultado<ResultadoPdfContratoDto>> GerarPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepositorio.ObterParaGeracaoPdfAsync(contratoId, cancellationToken);

        if (contrato is null)
            return Resultado<ResultadoPdfContratoDto>.CriarFalha("Contrato nÃ£o encontrado.");

        if (contrato.TenantId != empresaAtual.TenantId)
            return Resultado<ResultadoPdfContratoDto>.CriarFalha("Contrato nao encontrado.");

        var parceiro = await contabilidadeRepositorio.ObterAtualAsync(cancellationToken);
        var pendencias = ValidadorProntidaoContrato.ValidarParaGeracao(empresaAtual.TenantId, parceiro, contrato);
        if (pendencias.Count > 0)
            return Resultado<ResultadoPdfContratoDto>.CriarFalhaValidacao(pendencias, ContratoErros.DadosObrigatoriosPendentes);

        var instanteGeracao = timeProvider.GetUtcNow();
        var dataGeracao = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(instanteGeracao, "America/Sao_Paulo").DateTime;
        var dadosPdf = MapeadorDadosPdfContrato.Mapear(contrato, parceiro!, dataGeracao);

        await using var fluxoPdf = await servicoPdf.GerarContratoAsync(dadosPdf, cancellationToken);

        var caminho = await servicoArmazenamento.EnviarAsync(
            fluxoPdf,
            $"contratos/{contrato.Numero}.pdf",
            "contracts",
            cancellationToken);

        contrato.SetPdfPath(caminho);
        await contratoRepositorio.SalvarAlteracoesAsync(cancellationToken);
        await publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(contrato.TenantId, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id),
            cancellationToken);

        var url = await servicoArmazenamento.ObterUrlPreAssinadaAsync(caminho, TimeSpan.FromHours(1), cancellationToken);

        return Resultado<ResultadoPdfContratoDto>.CriarSucesso(
            new ResultadoPdfContratoDto(url, instanteGeracao.UtcDateTime));
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
        await publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(contrato.TenantId, EntidadesAtualizacaoTempoReal.Contrato, contrato.Id),
            cancellationToken);

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
            logger.LogWarning(
                "Webhook Autentique ignorado: nao foi possivel extrair tipo/documento. TamanhoPayload={TamanhoPayload}",
                payload.Length);
            return Resultado<bool>.CriarFalha("Payload do webhook Autentique invalido.");
        }

        logger.LogInformation(
            "Webhook Autentique recebido. Tipo={Tipo} DocumentoId={DocumentoId} DocumentoFinalizado={DocumentoFinalizado}",
            evento.Tipo,
            evento.DocumentoId,
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
            var conclusao = await FinalizarAssinaturaAsync(
                solicitacao,
                evento.DocumentoId,
                evento.OcorridoEm,
                documentoAssinadoJaArmazenado,
                ct);
            if (!conclusao.Sucesso)
                return conclusao;
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

    public async Task<Resultado<ResumoReconciliacaoAssinaturas>> ReconciliarAssinaturasAutentiqueAsync(
        CancellationToken ct = default)
    {
        var pendencias = await contratoRepositorio.ListarSolicitacoesAssinaturaParaReconciliacaoAsync(
            "autentique",
            ct);
        var concluidas = 0;
        var aindaPendentes = 0;
        var falhas = 0;

        foreach (var pendencia in pendencias)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var estado = await servicoAssinatura.ConsultarEstadoDocumentoAsync(pendencia.IdExterno, ct);
                if (!estado.Sucesso)
                {
                    falhas++;
                    logger.LogWarning(
                        "Falha ao consultar assinatura pendente. ContratoId={ContratoId} Erro={Erro}",
                        pendencia.ContratoId,
                        estado.Mensagem);
                    continue;
                }

                if (!estado.Concluido)
                {
                    aindaPendentes++;
                    continue;
                }

                var resultado = await ReconciliarConclusaoDocumentoAutentiqueAsync(
                    pendencia.IdExterno,
                    estado.ConcluidoEm,
                    ct);
                if (resultado.Sucesso)
                {
                    concluidas++;
                    continue;
                }

                falhas++;
                logger.LogWarning(
                    "Falha ao aplicar conclusao consultada na Autentique. ContratoId={ContratoId} Erro={Erro}",
                    pendencia.ContratoId,
                    resultado.Erro);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                falhas++;
                logger.LogError(
                    "Erro ao reconciliar assinatura. ContratoId={ContratoId} TipoExcecao={TipoExcecao} StackTrace={StackTrace}",
                    pendencia.ContratoId,
                    ex.GetType().FullName,
                    ex.StackTrace);
            }
        }

        var resumo = new ResumoReconciliacaoAssinaturas(
            pendencias.Count,
            concluidas,
            aindaPendentes,
            falhas);
        logger.LogInformation(
            "Reconciliacao Autentique finalizada. Consultadas={Consultadas} Concluidas={Concluidas} Pendentes={Pendentes} Falhas={Falhas}",
            resumo.Consultadas,
            resumo.Concluidas,
            resumo.Pendentes,
            resumo.Falhas);

        return Resultado<ResumoReconciliacaoAssinaturas>.CriarSucesso(resumo);
    }

    private async Task<Resultado<bool>> ReconciliarConclusaoDocumentoAutentiqueAsync(
        string documentoId,
        DateTime? concluidoEm,
        CancellationToken ct)
    {
        var solicitacao = await contratoRepositorio.ObterSolicitacaoAssinaturaPorIdExternoAsync(
            "autentique",
            documentoId,
            ct);
        if (solicitacao is null)
            return Resultado<bool>.CriarFalha("Solicitacao de assinatura nao encontrada para o documento Autentique.");

        empresaAtual.SetTenant(solicitacao.TenantId);
        var documentoAssinadoJaArmazenado = await contratoRepositorio.ExisteDocumentoAssinadoAsync(
            solicitacao.ContratoId,
            ct);

        return await FinalizarAssinaturaAsync(
            solicitacao,
            documentoId,
            concluidoEm,
            documentoAssinadoJaArmazenado,
            ct);
    }

    private async Task<Resultado<bool>> FinalizarAssinaturaAsync(
        AssinaturaSolicitacao solicitacao,
        string documentoId,
        DateTime? ocorridoEm,
        bool documentoAssinadoJaArmazenado,
        CancellationToken ct)
    {
        var conversao = await GarantirClienteDoContratoAsync(solicitacao.Contrato, ct);
        if (!conversao.Sucesso)
            return Resultado<bool>.CriarFalha(conversao.Erro!);

        var statusAssinado = await contratoRepositorio.ObterOuCriarStatusAssinaturaAsync(
            "assinado",
            "Assinado",
            90,
            ct);

        var dataConclusao = ocorridoEm ?? DateTime.UtcNow;
        var contratosAtivosAnteriores = await contratoRepositorio.ListarAtivosPorEmpresaAsync(
            solicitacao.Contrato.EmpresaId,
            ct);
        foreach (var contratoAtivo in contratosAtivosAnteriores.Where(
                     contrato => contrato.Id != solicitacao.ContratoId))
            contratoAtivo.Encerrar();

        solicitacao.MarcarConcluida(statusAssinado.Id, dataConclusao);
        solicitacao.Contrato.MarkAsSigned(dataConclusao);

        await contratoRepositorio.SalvarAlteracoesAsync(ct);
        await publicadorAtualizacaoTempoReal.PublicarAsync(
            new EventoAtualizacaoTempoReal(solicitacao.TenantId, EntidadesAtualizacaoTempoReal.Contrato, solicitacao.ContratoId),
            ct);

        if (!documentoAssinadoJaArmazenado)
        {
            var armazenamento = await ArmazenarPdfAssinadoAsync(solicitacao, documentoId, ct);
            if (!armazenamento.Sucesso)
                return Resultado<bool>.CriarFalha(armazenamento.Erro!);
        }

        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task<Resultado<bool>> GarantirClienteDoContratoAsync(
        Contrato contrato,
        CancellationToken ct)
    {
        var prospecto = await prospectoRepositorio.ObterComSociosAsync(contrato.ProspectId, ct);
        if (prospecto is null)
            return Resultado<bool>.CriarFalha("Prospecto do contrato nao encontrado.");

        var empresa = await GarantirEmpresaDoProspectoAsync(prospecto, ct);
        if (!empresa.Sucesso)
            return Resultado<bool>.CriarFalha(empresa.Erro!);

        contrato.VincularEmpresa(empresa.Valor!.Id);

        if (contrato.ClientId.HasValue)
            return Resultado<bool>.CriarSucesso(true);

        if (prospecto.ClientId is Guid clienteId)
        {
            var clienteExistente = await prospectoRepositorio.ObterClientePorProspectoAsync(prospecto.Id, ct);
            clienteExistente?.VincularEndereco(empresa.Valor.EnderecoId);
            contrato.VincularCliente(clienteId);
            return Resultado<bool>.CriarSucesso(true);
        }

        var cliente = await prospectoRepositorio.ObterClientePorProspectoAsync(prospecto.Id, ct);
        if (cliente is null)
        {
            if (prospecto.Status == StatusProspecto.Convertido)
                return Resultado<bool>.CriarFalha("Prospecto convertido sem cliente vinculado.");

            var statusCliente = await contratoRepositorio.ObterOuCriarStatusClienteAsync(
                "ATIVO",
                "Cliente ativo",
                1,
                ct);

            cliente = prospecto.ConverterParaCliente(statusCliente.Id);
            cliente.VincularEndereco(empresa.Valor.EnderecoId);
            await prospectoRepositorio.AdicionarAsync(cliente, ct);
        }
        else if (prospecto.Status != StatusProspecto.Convertido)
        {
            prospecto.MarkAsConverted(cliente.Id);
        }

        cliente.VincularEndereco(empresa.Valor.EnderecoId);
        contrato.VincularCliente(cliente.Id);
        return Resultado<bool>.CriarSucesso(true);
    }

    private async Task<Resultado<Empresa>> GarantirEmpresaDoProspectoAsync(
        Prospecto prospecto,
        CancellationToken ct)
    {
        var cnpj = DocumentoFiscal.SomenteDigitos(prospecto.Cnpj);
        if (cnpj?.Length != 14)
        {
            return Resultado<Empresa>.CriarFalha(
                "Informe o CNPJ e constitua a empresa antes de gerar o contrato.");
        }

        var empresa = await prospectoRepositorio.ObterEmpresaPorCnpjAsync(cnpj, ct);
        return await GarantirEmpresaDoProspectoAsync(prospecto, empresa, ct);
    }

    private async Task<Resultado<Empresa>> GarantirEmpresaDoProspectoAsync(
        Prospecto prospecto,
        Empresa? empresa,
        CancellationToken ct)
    {
        var cnpj = DocumentoFiscal.SomenteDigitos(prospecto.Cnpj);
        if (cnpj?.Length != 14)
        {
            return Resultado<Empresa>.CriarFalha(
                "Informe o CNPJ e constitua a empresa antes de gerar o contrato.");
        }

        if (empresa is null)
        {
            empresa = Empresa.CriarDoProspecto(prospecto);
            await prospectoRepositorio.AdicionarAsync(empresa, ct);
        }

        if (!empresa.EnderecoId.HasValue)
        {
            var resultadoEndereco = await servicoEnderecoComercial.CriarDoProspectoAsync(prospecto, ct);
            if (!resultadoEndereco.Sucesso)
                return Resultado<Empresa>.CriarFalha(resultadoEndereco.Erro!);

            empresa.VincularEndereco(resultadoEndereco.Valor?.Id);
        }

        if (prospecto.EmpresaId != empresa.Id)
            prospecto.VincularEmpresa(empresa.Id);

        return Resultado<Empresa>.CriarSucesso(empresa);
    }

    private static bool EhEventoConclusaoDocumento(EventoWebhookAutentique evento)
        => string.Equals(evento.Tipo, "document.finished", StringComparison.OrdinalIgnoreCase);

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
        await contratoRepositorio.SalvarAlteracoesAsync(ct);
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

    private static ContratoDto MapearContrato(Contrato contrato, bool pdfAssinadoDisponivel)
    {
        var razaoSocial = contrato.Cliente?.RazaoSocial ?? contrato.Prospecto.RazaoSocial;

        return new ContratoDto(
            contrato.Id,
            contrato.Numero,
            contrato.ProspectId,
            contrato.ClientId,
            contrato.BudgetId,
            razaoSocial,
            contrato.Status.ToString(),
            contrato.Orcamento?.ValorTotal ?? contrato.ValorTotal,
            contrato.DiaVencimentoComercial,
            contrato.DataInicio,
            contrato.DataFim,
            contrato.ClausulasExtras,
            !string.IsNullOrWhiteSpace(contrato.PdfPath),
            pdfAssinadoDisponivel,
            contrato.AssinadoEm,
            contrato.RepresentanteSocioId,
            contrato.DuracaoAnos,
            contrato.CompetenciaInicialHonorarios,
            contrato.ValorHonorarioMensal,
            contrato.ValorUnitarioPorFolha);
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
