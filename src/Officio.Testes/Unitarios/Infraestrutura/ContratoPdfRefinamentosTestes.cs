using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf;
using Officio.Infraestrutura.Pdf.Conteudo;
using Officio.Infraestrutura.Pdf.Formatacao;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Testes.Unitarios.Infraestrutura;

[Collection("PDF de contrato")]
public class ContratoPdfRefinamentosTestes
{
    private static readonly Lazy<XDocument[]> Paginas = new(() => Renderizar(ServicoPdfQuestTestes.CriarDados()));

    [Fact]
    public void PrimeiraPagina_NaoDeveExibirIntervaloDeVigenciaNaIntroducao()
    {
        var texto = Texto(Paginas.Value[0]);
        Assert.False(texto.Contains("Vigência:", StringComparison.OrdinalIgnoreCase));
        Assert.False(Regex.IsMatch(texto, @"\d{2}/\d{2}/\d{4}\s+a\s+\d{2}/\d{2}/\d{4}"), "Intervalo isolado de vigência na primeira página.");
    }

    [Theory]
    [InlineData(2025, 2, 13, 1, "01 (um) ano")]
    [InlineData(2027, 8, 19, 4, "04 (quatro) anos")]
    public void Clausula51_DeveUsarInicioPersistidoEDuracao(int ano, int mes, int dia, int duracao, string esperado)
    {
        var dados = ServicoPdfQuestTestes.CriarDados() with
        {
            DataInicio = new DateOnly(ano, mes, dia),
            DuracaoAnos = duracao
        };
        var modelo = Bloco("5.1 -");
        var texto = new FormatadorContrato(dados).Aplicar(modelo);
        Assert.True(texto.Contains(esperado), "Duração ausente na cláusula 5.1.");
        Assert.True(texto.Contains("contados a partir de " + dados.DataInicio.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
            "Início da vigência não corresponde à data persistida.");
        Assert.False(texto.Contains("a partir da sua assinatura"), "Cláusula 5.1 ainda usa a assinatura como início.");
        Assert.True(SemEspacos(Texto(Renderizar(dados))).Contains(SemEspacos(texto)), "Cláusula 5.1 não renderizada integralmente.");
    }

    [Fact]
    public void CorrecoesDePartes_DevemEstarRestritasAosPontosAutorizados()
    {
        var folha = Bloco("4.2.1 -");
        Assert.Equal(2, Regex.Matches(folha, @"\bCONTRATANTE\b").Count);
        Assert.Empty(Regex.Matches(folha, @"\bCONTRATADA\b"));
        var extras = Bloco("4.4 -");
        Assert.True(extras.Contains("prestados pela CONTRATADA"), "Prestadora incorreta na cláusula 4.4.");
        Assert.False(extras.Contains("prestados pela CONTRATANTE"), "Referência anterior ainda presente na cláusula 4.4.");
    }

    [Theory]
    [InlineData("João Exemplo", "Sócio administrador", "Empresário", "Carlos Técnico", "Contador")]
    [InlineData("Ana Exemplo", "Sócia administradora", "Empresária", "Maria Técnica", "Contadora")]
    public void Qualificacao_DeveFuncionarSemFlexoesFixasDoRepresentante(
        string nome, string relacao, string profissao, string responsavel, string profissaoTecnica)
    {
        var dados = ServicoPdfQuestTestes.CriarDados();
        dados = dados with
        {
            Contratante = dados.Contratante with
            {
                Representante = dados.Contratante.Representante with { Nome = nome, TipoRelacao = relacao, Profissao = profissao }
            },
            Contratada = dados.Contratada with
            {
                ResponsavelTecnico = dados.Contratada.ResponsavelTecnico with { Nome = responsavel, Profissao = profissaoTecnica }
            }
        };
        var formatador = new FormatadorContrato(dados);
        var contratante = formatador.Aplicar(ConteudoContrato.Abertura.Contratante);
        var contratada = formatador.Aplicar(ConteudoContrato.Abertura.Contratada);
        Assert.True(contratante.Contains($"representada por {nome}, {relacao}, {profissao}, CPF"), "Qualificação da contratante incorreta.");
        Assert.True(contratada.Contains($"representada por {responsavel}, {profissaoTecnica}, responsável técnico(a)"), "Qualificação da contratada incorreta.");
        foreach (var texto in new[] { contratante, contratada })
        {
            Assert.True(texto.Contains("com residência e domicílio"), "Qualificação sem construção neutra de residência.");
            Assert.False(Regex.IsMatch(texto, @"\b(pela|portadora|portador|domiciliada|domiciliado|Sra\.)\b"), "Flexão fixa de gênero na qualificação.");
        }
    }

    [Fact]
    public void Listas_DevemPreservarTextoOrdemEQuantidadeAnterioresAoRefinamento()
    {
        var itens = ConteudoContrato.Clausulas[3].Blocos.Where(b => b.Tipo == "Item").Select(b => b.Texto).ToArray();
        Assert.Equal(25, itens.Length);
        Assert.Equal("54a08d3e679e7784555f6a4a1043db27b6854a1bc3b54709ad9a66d303471eae", HashLista(itens));
        var areas = ConteudoContrato.Anexo.Areas;
        Assert.Equal(new[] { 22, 7 }, areas.Select(a => a.Itens.Length));
        Assert.Equal("7c01999759abe879546ffb4f8ac9c3c464483a5992362b3e400b5de3d57f815f", HashLista(areas[0].Itens));
        Assert.Equal("e6fc66560551ad2b7e468e109e53f054e510493723f036dfbad90431a3f0d1a3", HashLista(areas[1].Itens));
    }

    [Fact]
    public void Listas_DevemRenderizarBulletsESubitensNaHierarquiaOriginal()
    {
        var texto = SemEspacos(Texto(Paginas.Value));
        var inicioExtras = texto.IndexOf("4.4.2-", StringComparison.Ordinal);
        var fimExtras = texto.IndexOf("4.5-", inicioExtras, StringComparison.Ordinal);
        Assert.True(inicioExtras >= 0 && fimExtras > inicioExtras);
        Assert.Equal(25, texto[inicioExtras..fimExtras].Count(c => c == '•'));

        var inicioAnexo = texto.IndexOf(SemEspacos(ConteudoContrato.Anexo.Titulo), StringComparison.Ordinal);
        Assert.True(inicioAnexo >= 0);
        var anexo = texto[inicioAnexo..];
        var areas = ConteudoContrato.Anexo.Areas;
        for (var i = 0; i < areas.Length; i++)
        {
            var inicio = anexo.IndexOf(SemEspacos(areas[i].Titulo), StringComparison.Ordinal);
            var fim = i + 1 < areas.Length
                ? anexo.IndexOf(SemEspacos(areas[i + 1].Titulo), inicio, StringComparison.Ordinal) : anexo.Length;
            Assert.Equal(areas[i].Itens.Count(t => !t.StartsWith('-')), anexo[inicio..fim].Count(c => c == '•'));
        }

        var trechosAnexo = Paginas.Value.SkipWhile(p => !SemEspacos(Texto(p)).Contains(SemEspacos(ConteudoContrato.Anexo.Titulo)))
            .SelectMany(Trechos).ToArray();
        var bullets = trechosAnexo.Where(t => t.Value.Trim() == "•").Select(PosicaoX).ToArray();
        var subitens = trechosAnexo.Where(t => t.Value.TrimStart().StartsWith('-')).Select(PosicaoX).ToArray();
        Assert.Equal(19, bullets.Length);
        Assert.Equal(10, subitens.Length);
        Assert.True(subitens.All(x => x > bullets.Max() + 10), "Recuo dos subitens do anexo não preservado.");
    }

    [Fact]
    public void Fechamento_DeveDatarSomenteContratoPrincipalEManterAssinaturasDoAnexo()
    {
        var dados = ServicoPdfQuestTestes.CriarDados();
        var texto = SemEspacos(Texto(Paginas.Value));
        var data = SemEspacos($"Blumenau/SC, {dados.DataEmissao.ToString("dd 'de' MMMM 'de' yyyy", FormatadorContrato.Cultura)}.");
        var inicioAnexo = texto.IndexOf(SemEspacos(ConteudoContrato.Anexo.Titulo), StringComparison.Ordinal);
        var principal = texto[..inicioAnexo];
        var anexo = texto[inicioAnexo..];
        Assert.Single(Regex.Matches(principal, Regex.Escape(data)));
        Assert.False(anexo.Contains(data), "Data de geração repetida no anexo.");
        Assert.True(anexo.Contains(SemEspacos(dados.Contratante.Representante.Nome)), "Assinatura da contratante ausente no anexo.");
        Assert.True(anexo.Contains(SemEspacos(dados.Contratada.ResponsavelTecnico.Nome)), "Assinatura da contratada ausente no anexo.");
    }

    [Theory]
    [InlineData("Contabilidade Alfa", "CTR-ALFA-001")]
    [InlineData("Contabilidade Beta", "CTR-BETA-002")]
    public void CabecalhoERodape_DevemManterIdentificacaoDinamicaConfidencialidadeEPaginacao(string nome, string numero)
    {
        var dados = ServicoPdfQuestTestes.CriarDados();
        var paginas = Renderizar(dados with { NumeroContrato = numero, Contratada = dados.Contratada with { RazaoSocial = nome } });
        Assert.True(paginas.Length > 1);
        for (var i = 0; i < paginas.Length; i++)
        {
            var trechos = Trechos(paginas[i]).ToArray();
            var cabecalho = SemEspacos(string.Concat(trechos.Where(t => t.Attribute("font-size")?.Value == "9").Select(t => t.Value)));
            var rodape = SemEspacos(string.Concat(trechos.Where(t => t.Attribute("font-size")?.Value == "8").Select(t => t.Value)));
            Assert.True(cabecalho.Contains(SemEspacos(nome)) && cabecalho.Contains(SemEspacos(numero)), "Identificação dinâmica ausente no cabeçalho.");
            Assert.True(rodape.Contains(SemEspacos(numero)) && rodape.Contains("Documentoconfidencial."), "Rodapé incompleto.");
            Assert.True(rodape.Contains($"Página{i + 1}de{paginas.Length}"), "Paginação incorreta.");
        }
    }

    private static string Bloco(string prefixo) => ConteudoContrato.Clausulas.SelectMany(c => c.Blocos)
        .Single(b => b.Texto.StartsWith(prefixo, StringComparison.Ordinal)).Texto;

    private static XDocument[] Renderizar(DadosPdfContrato dados)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return new DocumentoContrato(dados).GenerateSvg().Select(XDocument.Parse).ToArray();
    }

    private static IEnumerable<XElement> Trechos(XDocument pagina) => pagina.Descendants().Where(e => e.Name.LocalName == "text");
    private static string Texto(params XDocument[] paginas) => string.Concat(paginas.SelectMany(Trechos).Select(e => e.Value));
    private static string SemEspacos(string texto) => Regex.Replace(texto, @"\s+", "");
    private static string HashLista(IEnumerable<string> itens) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", itens))));

    private static double PosicaoX(XElement trecho)
    {
        var transformacao = Regex.Match(trecho.Attribute("transform")!.Value, @"translate\(([-\d.]+)");
        return double.Parse(transformacao.Groups[1].Value, CultureInfo.InvariantCulture)
            + double.Parse(trecho.Attribute("x")!.Value.Split(',')[0], CultureInfo.InvariantCulture);
    }
}
