using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Enumeradores;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

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
    public async Task<ResultadoOperacao<Guid>> ConverterOrcamentoAsync(
        ConverterOrcamentoContratoRequisicao requisicao,
        CancellationToken ct = default)
    {
        var orcamento = await orcamentoRepositorio.ObterPorIdComItensAsync(requisicao.OrcamentoId, ct);
        if (orcamento is null)
            return ResultadoOperacao<Guid>.CriarFalha("Orcamento nao encontrado.");

        if (orcamento.Status != StatusOrcamento.Aceito)
            return ResultadoOperacao<Guid>.CriarFalha("Somente orcamentos aceitos podem ser convertidos em contrato.");

        if (await contratoRepositorio.ObterPorOrcamentoAsync(orcamento.Id, ct) is not null)
            return ResultadoOperacao<Guid>.CriarFalha("Ja existe contrato para este orcamento.");

        var prospecto = await prospectoRepositorio.ObterComSociosAsync(orcamento.ProspectId, ct);
        if (prospecto is null)
            return ResultadoOperacao<Guid>.CriarFalha("Prospecto nao encontrado.");

        var contrato = Contrato.CriarDeOrcamento(
            orcamento,
            prospecto,
            requisicao.DataInicio,
            requisicao.DataFim,
            requisicao.RenovacaoAutomatica,
            requisicao.ClausulasExtras);

        await contratoRepositorio.AdicionarAsync(contrato, ct);
        await contratoRepositorio.SalvarAlteracoesAsync(ct);

        return ResultadoOperacao<Guid>.CriarSucesso(contrato.Id);
    }

    public async Task<ResultadoOperacao<List<ContratoResultado>>> ListarAsync(CancellationToken ct = default)
    {
        var contratos = await contratoRepositorio.ListarAsync(ct);
        return ResultadoOperacao<List<ContratoResultado>>.CriarSucesso(contratos.Select(MapearContrato).ToList());
    }

    public async Task<ResultadoOperacao<ContratoResultado>> ObterPorIdAsync(Guid contratoId, CancellationToken ct = default)
    {
        var contrato = await contratoRepositorio.ObterDetalheAsync(contratoId, ct);
        if (contrato is null)
            return ResultadoOperacao<ContratoResultado>.CriarFalha("Contrato nao encontrado.");

        return ResultadoOperacao<ContratoResultado>.CriarSucesso(MapearContrato(contrato));
    }

    public async Task<ResultadoOperacao<PdfContratoResultado>> GerarPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepositorio.ObterParaGeracaoPdfAsync(contratoId, cancellationToken);

        if (contrato is null)
            return ResultadoOperacao<PdfContratoResultado>.CriarFalha("Contrato nÃ£o encontrado.");

        var empresa = await empresaRepositorio.ObterPorIdAsync(empresaAtual.TenantId, cancellationToken);
        if (empresa is null)
            return ResultadoOperacao<PdfContratoResultado>.CriarFalha("Empresa nÃ£o encontrada.");

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

        return ResultadoOperacao<PdfContratoResultado>.CriarSucesso(
            new PdfContratoResultado(url, DateTime.UtcNow));
    }

    public async Task<ResultadoOperacao<EnvioAssinaturaResultado>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepositorio.ObterParaAssinaturaAsync(requisicao.ContratoId, cancellationToken);

        if (contrato is null)
            return ResultadoOperacao<EnvioAssinaturaResultado>.CriarFalha("Contrato nÃ£o encontrado.");

        if (string.IsNullOrEmpty(contrato.PdfPath))
            return ResultadoOperacao<EnvioAssinaturaResultado>.CriarFalha(
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

            return ResultadoOperacao<EnvioAssinaturaResultado>.CriarSucesso(
                new EnvioAssinaturaResultado("manual_upload", null, null, resultado.Mensagem, pdfUrl));
        }

        contrato.SetD4SignEnvelope(resultado.EnvelopeId!);

        foreach (var signatario in requisicao.Signatarios)
        {
            var assinatura = Assinatura.Create(
                contrato.TenantId,
                contrato.Id,
                signatario.Nome,
                signatario.Email,
                signatario.Cpf,
                signatario.Tipo);

            await contratoRepositorio.AdicionarAssinaturaAsync(assinatura, cancellationToken);
        }

        await contratoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return ResultadoOperacao<EnvioAssinaturaResultado>.CriarSucesso(
            new EnvioAssinaturaResultado(
                "digital_api",
                resultado.EnvelopeId,
                resultado.LinksAssinantes,
                null,
                null));
    }

    private static DadosPdfContrato MapearDadosPdf(Contrato contrato, Empresa empresa)
    {
        var cliente = contrato.Cliente;
        var orcamento = contrato.Orcamento;

        var contratada = new DadosContratada(
            RazaoSocial: empresa.Nome,
            Cnpj: empresa.Cnpj ?? "",
            Endereco: "",
            Cidade: "",
            Uf: "",
            Email: "",
            Telefone: "");

        var endereco = string.Join(", ",
            new[] { cliente.Logradouro, cliente.Numero }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var contratante = new DadosContratante(
            RazaoSocial: cliente.RazaoSocial,
            NomeFantasia: cliente.NomeFantasia ?? cliente.RazaoSocial,
            Cnpj: cliente.Cnpj ?? "",
            RegimeTributario: cliente.RegimeTributario?.ToString() ?? "",
            Endereco: endereco,
            Cidade: cliente.Cidade ?? "",
            Uf: cliente.Uf ?? "",
            Email: cliente.Email ?? "",
            Telefone: cliente.Telefone ?? "");

        var socios = cliente.Socios
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

    private static ContratoResultado MapearContrato(Contrato contrato)
    {
        var razaoSocial = contrato.Cliente?.RazaoSocial ?? contrato.Prospecto.RazaoSocial;

        return new ContratoResultado(
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
}
