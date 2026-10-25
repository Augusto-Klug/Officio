using Officio.Infraestrutura.Pdf.Conteudo;
using Officio.Infraestrutura.Pdf.Formatacao;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteClausula(ClausulaContrato clausula, FormatadorContrato formatador) : IComponent
{
    public void Compose(IContainer container) => container.Column(col =>
    {
        col.Spacing(6);
        col.Item().EnsureSpace(60).Text(clausula.Titulo).Bold();
        foreach (var bloco in clausula.Blocos)
        {
            var texto = formatador.Aplicar(bloco.Texto);
            switch (bloco.Tipo)
            {
                case "Subtitulo":
                    col.Item().EnsureSpace(45).PaddingTop(4).Text(texto).Bold();
                    break;
                case "Item":
                    col.Item().PaddingLeft(12).Component(new ComponenteItemLista(texto));
                    break;
                case "Paragrafo":
                    col.Item().Text(texto).Justify();
                    break;
                default:
                    throw new InvalidOperationException($"Tipo de bloco contratual desconhecido: {bloco.Tipo}.");
            }
        }
    });
}
