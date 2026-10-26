using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf.Componentes;
using Officio.Infraestrutura.Pdf.Conteudo;
using Officio.Infraestrutura.Pdf.Formatacao;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf;

internal sealed class DocumentoContrato(DadosPdfContrato dados) : IDocument
{
    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Contrato {dados.NumeroContrato}",
        Author = dados.Contratada.RazaoSocial
    };

    public void Compose(IDocumentContainer container)
    {
        var formatador = new FormatadorContrato(dados);
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Lato).FontSize(10).LineHeight(1.25f));
            page.Header().Component(new ComponenteCabecalhoContrato(dados));
            page.Content().Column(col =>
            {
                col.Spacing(12);
                col.Item().Component(new ComponentePartes(formatador));
                foreach (var clausula in ConteudoContrato.Clausulas)
                    col.Item().Component(new ComponenteClausula(clausula, formatador));
                if (!string.IsNullOrWhiteSpace(dados.ClausulasExtras))
                    col.Item().Component(new ComponenteClausulasExtras(dados.ClausulasExtras));
                col.Item().EnsureSpace(70).Text(ConteudoContrato.Abertura.Fechamento).Justify();
                col.Item().Component(new ComponenteAssinaturas(dados, incluirTestemunhas: true));
                col.Item().PageBreak();
                col.Item().Component(new ComponenteAnexo(dados));
            });
            page.Footer().Component(new ComponenteRodapeContrato(dados.NumeroContrato));
        });
    }
}
