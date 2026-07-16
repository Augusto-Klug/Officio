using Microsoft.EntityFrameworkCore;
using Officio.Dominio.Entidades;
using Officio.Dominio.Contratos.Response;
using Officio.Dominio.Interfaces;
using Officio.Dominio.Contratos;

namespace Officio.Dominio.Servicos.Servicos;

internal class ContratoServico(
    DbContext db,
    IServicoPdf servicoPdf,
    IServicoAssinatura servicoAssinatura,
    IServicoArmazenamento servicoArmazenamento,
    IEmpresaAtual empresaAtual) : IContratoServico
{
    public async Task<ResultadoOperacao<PdfContratoResultado>> GerarPdfAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        var contrato = await db.Set<Contrato>()
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Socios)
            .Include(c => c.Orcamento)
                .ThenInclude(b => b!.Itens)
            .FirstOrDefaultAsync(c => c.Id == contratoId, cancellationToken);

        if (contrato is null)
            return ResultadoOperacao<PdfContratoResultado>.CriarFalha("Contrato nÃ£o encontrado.");

        var empresa = await db.Set<Empresa>()
            .FirstAsync(t => t.Id == empresaAtual.TenantId, cancellationToken);

        var dadosPdf = MapearDadosPdf(contrato, empresa);

        await using var fluxoPdf = await servicoPdf.GerarContratoAsync(dadosPdf, cancellationToken);

        var caminho = await servicoArmazenamento.EnviarAsync(
            fluxoPdf,
            $"contratos/{contrato.Numero}.pdf",
            "contracts",
            cancellationToken);

        contrato.SetPdfPath(caminho);
        await db.SaveChangesAsync(cancellationToken);

        var url = await servicoArmazenamento.ObterUrlPreAssinadaAsync(caminho, TimeSpan.FromHours(1), cancellationToken);

        return ResultadoOperacao<PdfContratoResultado>.CriarSucesso(
            new PdfContratoResultado(url, DateTime.UtcNow));
    }

    public async Task<ResultadoOperacao<EnvioAssinaturaResultado>> EnviarParaAssinaturaAsync(
        EnviarAssinaturaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contrato = await db.Set<Contrato>()
            .Include(c => c.Cliente)
            .FirstOrDefaultAsync(c => c.Id == requisicao.ContratoId, cancellationToken);

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

            db.Set<Assinatura>().Add(assinatura);
        }

        await db.SaveChangesAsync(cancellationToken);

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
}
