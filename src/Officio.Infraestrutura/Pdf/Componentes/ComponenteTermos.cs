using Officio.Dominio.Servicos.Interfaces;
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
            col.Item().Element(SectionTitle("4. DA VIGÃŠNCIA"));

            var vigencia = data.DataFim.HasValue
                ? $"{data.DataInicio:dd/MM/yyyy} a {data.DataFim.Value:dd/MM/yyyy}"
                : $"A partir de {data.DataInicio:dd/MM/yyyy}, por prazo indeterminado.";

            col.Item().PaddingTop(6).Text(vigencia).FontSize(9);

            if (data.RenovacaoAuto)
                col.Item().PaddingTop(4).Text(
                    "O presente contrato serÃ¡ renovado automaticamente ao final da vigÃªncia, " +
                    "salvo manifestaÃ§Ã£o expressa em contrÃ¡rio por qualquer das partes com " +
                    "antecedÃªncia mÃ­nima de 30 (trinta) dias.")
                    .FontSize(9).FontColor(Colors.Grey.Darken2);

            col.Item().PaddingTop(12).Element(SectionTitle("5. DAS OBRIGAÃ‡Ã•ES"));

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
         "A CONTRATADA se obriga a prestar os serviÃ§os contÃ¡beis descritos na ClÃ¡usula 2 " +
         "com zelo e diligÃªncia, em conformidade com as normas expedidas pelo Conselho Federal " +
         "de Contabilidade e demais legislaÃ§Ãµes aplicÃ¡veis."),

        ("5.2",
         "A CONTRATANTE se obriga a fornecer, em tempo hÃ¡bil, todos os documentos, " +
         "informaÃ§Ãµes e dados necessÃ¡rios ao cumprimento das obrigaÃ§Ãµes fiscais e trabalhistas, " +
         "sendo responsÃ¡vel pela veracidade das informaÃ§Ãµes prestadas."),

        ("5.3",
         "O nÃ£o pagamento dos honorÃ¡rios no prazo estipulado sujeitarÃ¡ a CONTRATANTE " +
         "Ã  multa de 2% (dois por cento) sobre o valor em atraso, acrescido de juros " +
         "de 1% (um por cento) ao mÃªs e correÃ§Ã£o monetÃ¡ria pelo IGPM."),

        ("5.4",
         "Qualquer das partes poderÃ¡ rescindir o presente contrato mediante notificaÃ§Ã£o " +
         "por escrito com antecedÃªncia mÃ­nima de 30 (trinta) dias, sem prejuÃ­zo do " +
         "pagamento dos honorÃ¡rios relativos ao perÃ­odo trabalhado."),
    ];

    static Action<IContainer> SectionTitle(string title) =>
        c => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
              .PaddingBottom(4)
              .Text(title).Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
}





