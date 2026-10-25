using Officio.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteCabecalhoContrato(DadosPdfContrato dados) : IComponent
{
    public void Compose(IContainer container) => container.PaddingBottom(12).Row(row =>
    {
        row.RelativeItem().Text(dados.Contratada.RazaoSocial).FontSize(9).Bold();
        row.RelativeItem().AlignRight().Text($"Contrato {dados.NumeroContrato}")
            .FontSize(9).FontColor(Colors.Grey.Darken2);
    });
}
