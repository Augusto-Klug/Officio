using Officio.Infraestrutura.Pdf.Conteudo;
using Officio.Infraestrutura.Pdf.Formatacao;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponentePartes(FormatadorContrato formatador) : IComponent
{
    public void Compose(IContainer container) => container.Column(col =>
    {
        col.Spacing(10);
        col.Item().Text(ConteudoContrato.Abertura.Titulo).Bold().FontSize(12).AlignCenter();
        col.Item().Text(formatador.Aplicar(ConteudoContrato.Abertura.Contratante)).Justify();
        col.Item().Text(formatador.Aplicar(ConteudoContrato.Abertura.Contratada)).Justify();
        col.Item().Text(ConteudoContrato.Abertura.Introducao).Justify();
    });
}
