using Officio.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

public class ComponenteTermos(DadosPdfContrato data) : IComponent
{
    public void Compose(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(SectionTitle("4. DA VIGÊNCIA"));

            var vigencia = data.DataFim.HasValue
                ? $"{data.DataInicio:dd/MM/yyyy} a {data.DataFim.Value:dd/MM/yyyy}"
                : $"A partir de {data.DataInicio:dd/MM/yyyy}, por prazo indeterminado.";

            col.Item().PaddingTop(6).Text(vigencia).FontSize(9);

            col.Item().PaddingTop(12).Element(SectionTitle("5. DAS OBRIGAÇÕES"));

            foreach (var (num, texto) in GetDefaultClauses())
            {
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.ConstantItem(24)
                       .Text($"{num}.").Bold().FontSize(9);
                    row.RelativeItem()
                       .Text(t =>
                       {
                           t.DefaultTextStyle(s => s.FontSize(9).LineHeight(1.4f));
                           t.Span(texto);
                       });
                });
            }
        });
    }

    static IEnumerable<(string Num, string Texto)> GetDefaultClauses() =>
    [
        ("5.1",
         "A CONTRATADA se obriga a prestar os serviços contábeis descritos na Cláusula 2 " +
         "com zelo e diligência, em conformidade com as normas expedidas pelo Conselho Federal " +
         "de Contabilidade e demais legislações aplicáveis."),

        ("5.2",
         "A CONTRATANTE se obriga a fornecer, em tempo hábil, todos os documentos, " +
         "informações e dados necessários ao cumprimento das obrigações fiscais e trabalhistas, " +
         "sendo responsável pela veracidade das informações prestadas."),

        ("5.3",
         "O não pagamento dos honorários no prazo estipulado sujeitará a CONTRATANTE " +
         "à multa de 2% (dois por cento) sobre o valor em atraso, acrescido de juros " +
         "de 1% (um por cento) ao mês e correção monetária pelo IGPM."),

        ("5.4",
         "Qualquer das partes poderá rescindir o presente contrato mediante notificação " +
         "por escrito com antecedência mínima de 30 (trinta) dias, sem prejuízo do " +
         "pagamento dos honorários relativos ao período trabalhado."),
    ];

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
}





