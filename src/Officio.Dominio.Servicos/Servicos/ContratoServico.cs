using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Contratos.Dtos;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;
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
    IEmpresaAtual empresaAtual) : IContratoServico
{
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
        return Resultado<List<ContratoDto>>.CriarSucesso(contratos.Select(MapearContrato).ToList());
    }

    public async Task<Resultado<ContratoDto>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default)
    {
        var contrato = await contratoRepositorio.ObterDetalheAsync(contratoId, ct);
        if (contrato is null)
            return Resultado<ContratoDto>.CriarFalha("Contrato nao encontrado.");

        return Resultado<ContratoDto>.CriarSucesso(MapearContrato(contrato));
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

        var solicitacao = new SolicitacaoAssinatura(fluxoPdf, contrato.Numero, assinantes);
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
            return Resultado<bool>.CriarFalha("Payload do webhook Autentique invalido.");

        var solicitacao = await contratoRepositorio.ObterSolicitacaoAssinaturaPorIdExternoAsync(
            "autentique",
            evento.DocumentoId,
            ct);

        if (solicitacao is null)
            return Resultado<bool>.CriarFalha("Solicitacao de assinatura nao encontrada.");

        if (await contratoRepositorio.ExisteEventoAssinaturaAsync(
                solicitacao.Id,
                evento.Tipo,
                payload,
                ct))
        {
            return Resultado<bool>.CriarSucesso(true);
        }

        await contratoRepositorio.AdicionarEventoAssinaturaAsync(
            AssinaturaEvento.Criar(solicitacao.TenantId, solicitacao.Id, evento.Tipo, payload),
            ct);

        if (evento.Tipo == "document.finished")
        {
            var statusAssinado = await contratoRepositorio.ObterOuCriarStatusAssinaturaAsync(
                "assinado",
                "Assinado",
                90,
                ct);

            solicitacao.MarcarConcluida(statusAssinado.Id, evento.OcorridoEm ?? DateTime.UtcNow);
            solicitacao.Contrato.MarkAsSigned();
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

    private static ContratoDto MapearContrato(Contrato contrato)
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
            contrato.ValorTotal,
            contrato.DiaVencimento,
            contrato.DataInicio,
            contrato.DataFim,
            contrato.RenovacaoAutomatica,
            contrato.ClausulasExtras,
            !string.IsNullOrWhiteSpace(contrato.PdfPath));
    }

    private sealed record EventoWebhookAutentique(
        string Tipo,
        string DocumentoId,
        string? EmailSignatario,
        DateTime? OcorridoEm)
    {
        public static EventoWebhookAutentique? Criar(string payload)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                var root = document.RootElement;
                var tipo = ObterString(root, "event", "type", "evento", "tipo");
                var documentoId = ObterDocumentoId(root);

                if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(documentoId))
                    return null;

                return new EventoWebhookAutentique(
                    tipo,
                    documentoId,
                    ObterEmailSignatario(root),
                    ObterData(root));
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? ObterDocumentoId(JsonElement root)
        {
            var direto = ObterString(root, "document_id", "documentId");
            if (!string.IsNullOrWhiteSpace(direto))
                return direto;

            if (root.TryGetProperty("document", out var document))
                return ObterString(document, "id", "uuid");

            if (root.TryGetProperty("data", out var data))
            {
                var dataDireto = ObterString(data, "document_id", "documentId");
                if (!string.IsNullOrWhiteSpace(dataDireto))
                    return dataDireto;

                if (data.TryGetProperty("document", out var dataDocument))
                    return ObterString(dataDocument, "id", "uuid");
            }

            return null;
        }

        private static string? ObterEmailSignatario(JsonElement root)
        {
            if (root.TryGetProperty("signature", out var signature))
                return ObterString(signature, "email");

            if (root.TryGetProperty("signer", out var signer))
                return ObterString(signer, "email");

            if (root.TryGetProperty("data", out var data))
                return ObterEmailSignatario(data);

            return ObterString(root, "email", "signer_email", "signerEmail");
        }

        private static DateTime? ObterData(JsonElement root)
        {
            var data = ObterString(root, "created_at", "createdAt", "occurred_at", "occurredAt");
            return DateTime.TryParse(data, out var valor) ? valor.ToUniversalTime() : null;
        }

        private static string? ObterString(JsonElement element, params string[] nomes)
        {
            foreach (var nome in nomes)
            {
                if (element.TryGetProperty(nome, out var valor)
                    && valor.ValueKind == JsonValueKind.String)
                    return valor.GetString();
            }

            return null;
        }
    }
}
