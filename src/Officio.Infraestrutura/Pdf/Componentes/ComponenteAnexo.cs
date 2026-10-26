using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf.Conteudo;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteAnexo(DadosPdfContrato dados) : IComponent
{
    public void Compose(IContainer container) => container.Column(col =>
    {
        col.Spacing(7);
        col.Item().EnsureSpace(80).Text(ConteudoContrato.Anexo.Titulo).Bold().AlignCenter();
        foreach (var area in ConteudoContrato.Anexo.Areas)
        {
            col.Item().EnsureSpace(60).PaddingTop(8).Text(area.Titulo).Bold();
            foreach (var item in area.Itens)
            {
                if (item.StartsWith('-'))
                    col.Item().PaddingLeft(12).Text(item).Justify();
                else
                    col.Item().Component(new ComponenteItemLista(item));
            }
        }
        col.Item().PaddingTop(18).Component(new ComponenteAssinaturas(
            dados, incluirTestemunhas: false, incluirData: false));
    });
}
