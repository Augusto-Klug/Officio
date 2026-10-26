using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf;
using Officio.Infraestrutura.Pdf.Conteudo;
using Officio.Infraestrutura.Pdf.Formatacao;
using Officio.Infraestrutura.Servicos;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Testes.Unitarios.Infraestrutura;

[Collection("PDF de contrato")]
public class ServicoPdfQuestTestes
{
    [Fact]
    public void ConteudoFixo_DevePreservarIntegridadeDaReferencia()
    {
        using var baseline = typeof(ServicoPdfQuestTestes).Assembly.GetManifestResourceStream(
            "Officio.Testes.Fixtures.contrato-integridade.json")!;
        var esperado = JsonSerializer.Deserialize<Dictionary<string, string>>(baseline)!;
        foreach (var (nome, hash) in esperado)
        {
            using var recurso = typeof(ServicoPdfQuest).Assembly.GetManifestResourceStream(
                $"Officio.Infraestrutura.Pdf.Conteudo.{nome}.json");
            Assert.NotNull(recurso);
            using var leitor = new StreamReader(recurso);
            var conteudo = leitor.ReadToEnd().Replace("\r\n", "\n");
            Assert.True(Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo))) == hash,
                $"Integridade do recurso {nome} divergente da referência conferida.");
        }
        Assert.Equal(7, ConteudoContrato.Clausulas.Length);
        Assert.Equal(new[] { 1, 24, 12, 50, 18, 7, 1 }, ConteudoContrato.Clausulas.Select(c => c.Blocos.Length));
        Assert.Equal(new[] { 22, 7 }, ConteudoContrato.Anexo.Areas.Select(a => a.Itens.Length));
    }

    [Fact]
    public void Modelos_DevemSubstituirTodosOsMarcadoresSemReprocessarDadosDoUsuario()
    {
        var dados = CriarDados();
        var formatador = new FormatadorContrato(dados);
        var modelos = ConteudoContrato.Clausulas.SelectMany(c => c.Blocos).Select(b => b.Texto)
            .Concat([ConteudoContrato.Abertura.Contratante, ConteudoContrato.Abertura.Contratada]);
        Assert.True(modelos.All(m => !formatador.Aplicar(m).Contains("{{")), "Marcador não resolvido no contrato.");
        Assert.Throws<InvalidOperationException>(() => formatador.Aplicar("{{Desconhecido}}"));
        var textoLiteral = new FormatadorContrato(dados with
        {
            Contratante = dados.Contratante with { RazaoSocial = "Empresa {{HonorarioMensal}} & Filhos" }
        }).Aplicar("{{ContratanteRazaoSocial}}");
        Assert.Equal("Empresa {{HonorarioMensal}} & Filhos", textoLiteral);
    }

    [Theory]
    [InlineData("0", "zero reais")]
    [InlineData("0.01", "um centavo")]
    [InlineData("1", "um real")]
    [InlineData("30", "trinta reais")]
    [InlineData("450", "quatrocentos e cinquenta reais")]
    [InlineData("1234.56", "mil duzentos e trinta e quatro reais e cinquenta e seis centavos")]
    [InlineData("1000000", "um milhão de reais")]
    [InlineData("1.005", "um real e um centavo")]
    public void Moeda_DeveUsarExtensoCorrespondenteAoValor(string valor, string esperado)
        => Assert.Equal(esperado, NumeroPorExtenso.Moeda(decimal.Parse(valor, CultureInfo.InvariantCulture)));

    [Theory]
    [InlineData(1, "01 (um) ano")]
    [InlineData(4, "04 (quatro) anos")]
    public void Duracao_DeveIncluirAUnidadeDeTempo(int anos, string esperado)
        => Assert.Equal(esperado, new FormatadorContrato(CriarDados() with { DuracaoAnos = anos }).Aplicar("{{Duracao}}"));

    [Theory]
    [InlineData("en-US")]
    [InlineData("th-TH")]
    public void Formatacao_DeveSerBrasileiraIndependentementeDaCulturaAtual(string cultura)
    {
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultura);
            var texto = new FormatadorContrato(CriarDados()).Aplicar("{{HonorarioMensal}} | {{Competencia}} | {{Vencimento}} | {{Duracao}} | {{PrecoPorFolha}}");
            Assert.Equal("R$ 987,65 (novecentos e oitenta e sete reais e sessenta e cinco centavos) | 11/2026 | 17 (dezessete) | 03 (três) anos | R$ 42,35 (quarenta e dois reais e trinta e cinco centavos)", texto);
        }
        finally { CultureInfo.CurrentCulture = anterior; }
    }

    [Fact]
    public async Task Geracao_DeveProduzirPdfPaginadoEPermitirPreviaLocal()
    {
        var dados = CriarDados();
        var servico = new ServicoPdfQuest(NullLogger<ServicoPdfQuest>.Instance);
        await using var stream = await servico.GerarContratoAsync(dados);
        Assert.True(stream.CanRead && stream.CanSeek);
        Assert.Equal(0, stream.Position);
        Assert.True(stream.Length > 10_000);
        var assinatura = new byte[5];
        await stream.ReadExactlyAsync(assinatura);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(assinatura));

        var documento = new DocumentoContrato(dados);
        var paginas = documento.GenerateSvg().ToArray();
        Assert.InRange(paginas.Length, 5, 20);
        Assert.True(paginas.All(p => p.Contains("<svg")), "Página inválida na renderização.");

        var destino = Environment.GetEnvironmentVariable("OFFICIO_PDF_PREVIEW_DIR");
        if (!string.IsNullOrWhiteSpace(destino))
        {
            Directory.CreateDirectory(destino);
            stream.Position = 0;
            await using var arquivo = File.Create(Path.Combine(destino, "contrato-exemplo.pdf"));
            await stream.CopyToAsync(arquivo);
            documento.GenerateImages(i => Path.Combine(destino, $"pagina-{i + 1:00}.png"),
                new ImageGenerationSettings { RasterDpi = 110 });
            for (var i = 0; i < paginas.Length; i++)
                await File.WriteAllTextAsync(Path.Combine(destino, $"pagina-{i + 1:00}.svg"), paginas[i]);
        }
    }

    [Fact]
    public void Renderizacao_DeveConterTodoConteudoNaOrdemDaReferencia()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = CriarDados();
        var formatador = new FormatadorContrato(dados);
        var paginas = new DocumentoContrato(dados).GenerateSvg().Select(XDocument.Parse).ToArray();
        var trechos = paginas.SelectMany(p => p.Descendants().Where(e => e.Name.LocalName == "text"))
            .Where(e => float.Parse(e.Attribute("font-size")!.Value, CultureInfo.InvariantCulture) >= 10);
        var renderizado = SemEspacos(string.Concat(trechos.Select(e => e.Value)));
        var esperados = new List<string>
        {
            ConteudoContrato.Abertura.Titulo,
            formatador.Aplicar(ConteudoContrato.Abertura.Contratante),
            formatador.Aplicar(ConteudoContrato.Abertura.Contratada),
            ConteudoContrato.Abertura.Introducao
        };
        foreach (var clausula in ConteudoContrato.Clausulas)
        {
            esperados.Add(clausula.Titulo);
            esperados.AddRange(clausula.Blocos.Select(b => formatador.Aplicar(b.Texto)));
        }
        esperados.Add(ConteudoContrato.Abertura.Fechamento);
        esperados.Add(ConteudoContrato.Anexo.Titulo);
        foreach (var area in ConteudoContrato.Anexo.Areas)
        {
            esperados.Add(area.Titulo);
            esperados.AddRange(area.Itens);
        }
        var posicao = 0;
        for (var i = 0; i < esperados.Count; i++)
        {
            var esperado = SemEspacos(esperados[i]);
            var encontrado = renderizado.IndexOf(esperado, posicao, StringComparison.Ordinal);
            Assert.True(encontrado >= 0, $"Bloco de índice {i} ausente ou fora de ordem na renderização.");
            posicao = encontrado + esperado.Length;
        }
    }

    private static string SemEspacos(string texto) => Regex.Replace(texto, @"\s+", "");

    [Fact]
    public async Task Geracao_ComCamposLongosEClausulasExtras_DevePaginarSemErro()
    {
        var dados = CriarDados();
        var longo = string.Join(" ", Enumerable.Repeat("Texto extenso para verificar quebra de página e acentuação.", 120));
        dados = dados with
        {
            ClausulasExtras = longo,
            Contratante = dados.Contratante with { RazaoSocial = string.Concat(Enumerable.Repeat("Empresa Exemplo de Nome Longo ", 8)) }
        };
        await using var stream = await new ServicoPdfQuest(NullLogger<ServicoPdfQuest>.Instance).GerarContratoAsync(dados);
        Assert.True(stream.Length > 10_000);
    }

    [Fact]
    public async Task Geracao_Cancelada_NaoDeveProduzirArquivo()
    {
        using var ct = new CancellationTokenSource();
        ct.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ServicoPdfQuest(NullLogger<ServicoPdfQuest>.Instance).GerarContratoAsync(CriarDados(), ct.Token));
    }

    internal static DadosPdfContrato CriarDados()
    {
        var endereco = new DadosEnderecoContrato("Rua Exemplo", "123", "Sala 4", "Centro", "89010000", "Blumenau", "SC");
        return new DadosPdfContrato
        {
            NumeroContrato = "CTR-2026-PREVIA",
            DataEmissao = new DateTime(2026, 10, 24, 10, 0, 0),
            Contratada = new DadosContratada("Contabilidade Exemplo Ltda.", "12345678000195", "12345/O-0", endereco,
                new DadosResponsavelTecnico("Maria Técnica Exemplo", "Contadora", "12345678901", "54321/O-0", endereco)),
            Contratante = new DadosContratante("Empresa de Demonstração Ltda.", "Demonstração", "11222333000144", "SimplesNacional", endereco, null, null,
                new DadosRepresentanteContrato("Sócio administrador", "João Representante Exemplo", "Empresário", "98765432100", endereco)),
            DiaVencimento = 17,
            DataInicio = new DateOnly(2026, 11, 1),
            DataFim = new DateOnly(2029, 11, 1),
            DuracaoAnos = 3,
            CompetenciaInicialHonorarios = new DateOnly(2026, 11, 1),
            ValorHonorarioMensal = 987.65m,
            ValorUnitarioPorFolha = 42.35m
        };
    }
}
