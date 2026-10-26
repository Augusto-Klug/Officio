using Officio.Dominio.Interfaces;
using Officio.Infraestrutura.Pdf.Formatacao;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Officio.Infraestrutura.Pdf.Componentes;

internal sealed class ComponenteAssinaturas(
    DadosPdfContrato dados, bool incluirTestemunhas, bool incluirData = true) : IComponent
{
    public void Compose(IContainer container) => container.ShowEntire().Column(col =>
    {
        col.Spacing(8);
        if (incluirData)
            col.Item().Text($"{FormatadorContrato.CidadeUf(dados.Contratada.Endereco)}, {dados.DataEmissao.ToString("dd 'de' MMMM 'de' yyyy", FormatadorContrato.Cultura)}.");
        col.Item().PaddingTop(28).Row(row =>
        {
            row.RelativeItem().Element(c => Assinante(c, dados.Contratante.RazaoSocial,
                dados.Contratante.Representante.Nome, dados.Contratante.Representante.Cpf));
            row.ConstantItem(24);
            row.RelativeItem().Element(c => Assinante(c, dados.Contratada.RazaoSocial,
                dados.Contratada.ResponsavelTecnico.Nome, dados.Contratada.ResponsavelTecnico.Cpf));
        });
        if (incluirTestemunhas)
        {
            col.Item().PaddingTop(12).Text("TESTEMUNHAS:").Bold();
            col.Item().PaddingTop(24).Row(row =>
            {
                row.RelativeItem().Element(Testemunha);
                row.ConstantItem(24);
                row.RelativeItem().Element(Testemunha);
            });
        }
    });

    private static void Assinante(IContainer container, string empresa, string nome, string cpf)
        => container.BorderTop(0.5f).PaddingTop(6).Column(col =>
        {
            col.Item().Text(empresa).Bold();
            col.Item().Text(nome);
            col.Item().Text($"CPF: {FormatadorContrato.Documento(cpf)}");
        });

    private static void Testemunha(IContainer container) => container.BorderTop(0.5f).PaddingTop(6).Column(col =>
    {
        col.Item().Text("Nome:");
        col.Item().Text("CPF:");
    });
}
